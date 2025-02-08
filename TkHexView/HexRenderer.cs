using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RogueRender.Resources;

namespace RogueRender;

public class HexRenderer
{
	private const int MaxCells = 16384;
	private readonly Cell[] _cells = new Cell[MaxCells];
	private int _cellCursor = 0;

	public const int FontSize = 16;
	public const int FontCharWidth = 8;
	public const int FontCharHeight = 19;
	public const int FontCharPadding = 2;
	private ShaderProgram _shader;

	private Dictionary<char, GlyphMetrics> _glyphMetrics = new();

	private const int CellWidth = FontCharWidth / 2;

	public readonly int BytesPerRow = 16;
	public readonly int Rows = 64;

	private static readonly char[] NibbleChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

	public void Setup()
	{
		GL.ClearColor(0f, 0f, 0f, 1f);

		GL.DebugMessageCallback(OnDebugMessage, IntPtr.Zero);
		GL.Enable(EnableCap.DebugOutput);

		GL.Enable(EnableCap.DebugOutputSynchronous);

		LoadFont("font.png");
		LoadShaders();
		CreateVbo();

		GL.Uniform1i(
			_shader.GetUniform("border_size"),
			1
		);

		GL.Uniform1i(
			_shader.GetUniform("dash_size"),
			1
		);

		GL.Uniform2f(
			_shader.GetUniform("cell_size"),
			FontCharWidth,
			FontCharHeight
		);
	}

	private static void OnDebugMessage(DebugSource source, DebugType type, uint id, DebugSeverity severity, int messageLength, IntPtr messagePtr, IntPtr userParamPtr)
	{
		var message = Marshal.PtrToStringAnsi(messagePtr, messageLength);
		Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);

		if (type == DebugType.DebugTypeError)
			throw new Exception(message);
	}

	public void Resize(int width, int height)
	{
		GL.Viewport(0, 0, width, height);

		GL.UniformMatrix4f(
			_shader.GetUniform("mvp"),
			1,
			false,
			Matrix4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1)
		);
	}

	private void SubmitCell(Vector2i position, Color3<Rgb> background, Color3<Rgb> foreground, char character, CellFlags flags)
	{
		if (!_glyphMetrics.TryGetValue(character, out var metrics))
			metrics = new GlyphMetrics(Vector2.Zero, Vector2.Zero, Vector2.Zero);

		_cells[_cellCursor++] = new Cell
		{
			TextureOpacity = byte.MaxValue,
			TextureOffset = new Vector2h(metrics.TextureOffset.X, metrics.TextureOffset.Y),
			TextureSize = new Vector2h(metrics.TextureSize.X, metrics.TextureSize.Y),
			Position = position * new Vector2i(CellWidth, FontCharHeight),
			Foreground = foreground,
			Background = background,
			SelectedColor = new Color4<Rgba>(0, 0.3f, 1, 0.6f),
			BorderColor = Color4.Yellow,
			Width = 1,
			Flags = flags
		};
	}

	private void SubmitRectangle(Vector2i position, byte span, Color3<Rgb> color, CellFlags flags)
	{
		_cells[_cellCursor++] = new Cell
		{
			TextureOpacity = byte.MinValue,
			Position = position * new Vector2i(CellWidth, FontCharHeight),
			Foreground = Color3.Black,
			Background = color,
			SelectedColor = new Color4<Rgba>(0, 0.3f, 1, 0.6f),
			BorderColor = Color4.Yellow,
			Width = span,
			Flags = flags
		};
	}

	public void Submit(ReadOnlySpan<byte> hexData)
	{
		_cellCursor = 0;

		const CellFlags nibbleFlags = ~(CellFlags.LeftBorder | CellFlags.RightBorder);

		for (var i = 0; i < hexData.Length; i++)
		{
			var b = hexData[i];
			var normalizedByte = b / 255f;

			var x = i % BytesPerRow;
			var y = i / BytesPerRow;

			var cellBg = new Color3<Rgb>(normalizedByte, normalizedByte, normalizedByte);

			var flags = CellFlags.None;

			if (i == 75)
				flags |= CellFlags.LeftBorder;

			if (i is >= 75 and <= 100)
			{
				flags |= CellFlags.TopBorder;
				flags |= CellFlags.BottomBorder;
			}

			if (i == 100)
				flags |= CellFlags.RightBorder;

			if (i is > 120 and < 135)
				flags |= CellFlags.Selected;

			// 6 cells across
			var cellX = x * 6;

			// Background
			SubmitRectangle(
				new Vector2i(cellX, y),
				3,
				cellBg,
				flags
			);

			// High nibble
			SubmitCell(
				new Vector2i(cellX + 1, y),
				cellBg,
				normalizedByte > 0.5 ? Color3.Black : Color3.White,
				NibbleChars[b >> 4],
				flags & nibbleFlags
			);

			// Low nibble
			SubmitCell(
				new Vector2i(cellX + 3, y),
				cellBg,
				normalizedByte > 0.5 ? Color3.Black : Color3.White,
				NibbleChars[b & 0xF],
				flags & nibbleFlags
			);

			var c = (char)b;
			var color = Color3.White;

			if (b is < 32 or > 127)
			{
				c = '.';
				color = Color3.Darkslategray;
			}

			// Preview character
			SubmitCell(
				new Vector2i(BytesPerRow * 6 + 4 + x * 2, y),
				Color3.Black,
				color,
				c,
				flags | CellFlags.DashedBorder
			);
		}

		var vertexSpan = (ReadOnlySpan<Cell>)_cells;
		GL.BufferSubData(BufferTarget.ArrayBuffer, 0, vertexSpan[.._cellCursor]);
	}

	public void Render()
	{
		GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.DrawArrays(PrimitiveType.Points, 0, _cellCursor);
	}

	private void LoadFont(string resourceFilename)
	{
		var id = GL.GenTexture();
		GL.BindTexture(TextureTarget.Texture2d, id);

		var image = new Bitmap(ResourceHelper.Get(resourceFilename));
		var bits = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

		GL.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, bits.Width, bits.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);

		image.UnlockBits(bits);

		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
		GL.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

		var imageSize = new Vector2(image.Width, image.Height);

		for (var i = 32; i < 127; i++)
		{
			var x = (FontCharWidth + FontCharPadding) * (i % 16) + FontCharPadding / 2;
			var y = (FontCharHeight + FontCharPadding) * (i / 16) + FontCharPadding / 2;

			_glyphMetrics[(char)i] = new GlyphMetrics(
				new Vector2(x, y) / imageSize,
				new Vector2(FontCharWidth, FontCharHeight) / imageSize,
				new Vector2(0, 0)
			);
		}
	}

	private void LoadShaders()
	{
		_shader = new ShaderProgram();

		_shader.AttachShader(ShaderType.VertexShader, ResourceHelper.Read("characters.vert"));
		_shader.AttachShader(ShaderType.GeometryShader, ResourceHelper.Read("characters.geom"));
		_shader.AttachShader(ShaderType.FragmentShader, ResourceHelper.Read("characters.frag"));

		_shader.BindStructAttribLocations<Cell>(s => s switch
		{
			nameof(Cell.Position) => "position",
			nameof(Cell.Width) => "width",
			nameof(Cell.TextureOpacity) => "texture_opacity",
			nameof(Cell.TextureOffset) => "texture_offset",
			nameof(Cell.TextureSize) => "texture_size",
			nameof(Cell.Foreground) => "foreground",
			nameof(Cell.Background) => "background",
			nameof(Cell.BorderColor) => "border_color",
			nameof(Cell.SelectedColor) => "selected_color",
			_ => null
		});

		_shader.Link();
		_shader.Use();
	}

	private void CreateVbo()
	{
		GL.GenVertexArray(out var vao);
		GL.BindVertexArray(vao);

		GL.GenBuffer(out var vertexBuffer);
		GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
		GL.BufferData(BufferTarget.ArrayBuffer, _cells, BufferUsage.StreamDraw);

		Buffer.BindStructAttributes<Cell>();
	}
}