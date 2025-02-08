using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace RogueRender;

public class HexWindow : GameWindow
{
	private readonly HexRenderer _renderer = new();

	private readonly byte[] _hexData;

	private const int MaxFrame = 2000;
	private uint _frame = 0;
	private double[] _frameTimes = new double[MaxFrame];

	public HexWindow() : base(new GameWindowSettings(), new NativeWindowSettings()
	{
		ClientSize = new Vector2i(1280, 720),
		Flags = ContextFlags.Debug
	})
	{
		VSync = VSyncMode.On;

		_hexData = new byte[_renderer.BytesPerRow * _renderer.Rows];
		new Random().NextBytes(_hexData);
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		_renderer.Setup();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		_renderer.Resize(ClientSize.X, ClientSize.Y);
	}

	protected override void OnUpdateFrame(FrameEventArgs args)
	{
		base.OnUpdateFrame(args);
		_renderer.Submit(_hexData);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		_renderer.Render();

		SwapBuffers();

		Title = $"FrameTime: {(int)(args.Time * 10000) / 10f}ms";

		// if (_frame >= MaxFrame + 1000)
		// {
		// 	using (var sw = new StreamWriter("frame_times.txt"))
		// 		foreach (var time in _frameTimes)
		// 			sw.WriteLine(time);
		// 	
		// 	Environment.Exit(0);
		// }
		// else if (_frame >= 1000)
		// {
		// 	_frameTimes[_frame - 1000] = args.Time * 1_000_000; // microseconds
		// }
		//
		// _frame++;
	}
}