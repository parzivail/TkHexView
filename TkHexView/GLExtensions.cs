using OpenTK.Graphics.OpenGL;

namespace TkHexView;

public static class GLExtensions
{
	public static unsafe void BufferSubData<T1>(BufferTarget target, IntPtr offset, ReadOnlySpan<T1> data) where T1 : unmanaged
	{
		nint size = data.Length * sizeof(T1);
		fixed (void* data_ptr = data)
		{
			GL.BufferSubData(target, offset, size, data_ptr);
		}
	}
	
	public static unsafe void BufferData<T1>(BufferTarget target, T1[] data, BufferUsage usage) where T1 : unmanaged
	{
		nint size = data.Length * sizeof(T1);
		fixed (void* data_ptr = data)
		{
			GL.BufferData(target, size, data_ptr, usage);
		}
	}
}