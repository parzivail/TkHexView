using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace TkHexView;

[Flags]
public enum CellFlags : byte
{
	None = 0,
	Selected = 1 << 0,
	LeftBorder = 1 << 1,
	TopBorder = 1 << 2,
	RightBorder = 1 << 3,
	BottomBorder = 1 << 4,
	DashedBorder = 1 << 5,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Cell
{
	public static readonly int Size = Marshal.SizeOf<Cell>();

	[BufferAttribute(1, VertexAttribPointerType.UnsignedByte, uploadAsInteger: true)]
	public CellFlags Flags;

	[BufferAttribute(1, VertexAttribPointerType.UnsignedByte, normalize: true)]
	public byte TextureOpacity;

	[BufferAttribute(2, VertexAttribPointerType.Float)]
	public Vector2 Position;

	[BufferAttribute(1, VertexAttribPointerType.UnsignedByte)]
	public byte Width;

	[BufferAttribute(2, VertexAttribPointerType.HalfFloat)]
	public Vector2h TextureOffset;

	[BufferAttribute(2, VertexAttribPointerType.HalfFloat)]
	public Vector2h TextureSize;

	[BufferAttribute(3, VertexAttribPointerType.Float)]
	public Color3<Rgb> Foreground;

	[BufferAttribute(3, VertexAttribPointerType.Float)]
	public Color3<Rgb> Background;

	[BufferAttribute(4, VertexAttribPointerType.Float)]
	public Color4<Rgba> BorderColor;

	[BufferAttribute(4, VertexAttribPointerType.Float)]
	public Color4<Rgba> SelectedColor;
}