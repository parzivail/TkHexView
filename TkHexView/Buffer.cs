using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace TkHexView;

public class Buffer
{
	public static void BindStructAttributes<T>() where T : struct
	{
		var structSize = Marshal.SizeOf<T>();

		var t = typeof(T);
		foreach (var (index, field, attr) in BufferUtil.GetBufferAttributes(t))
		{
			var offset = Marshal.OffsetOf<T>(field.Name);

			GL.EnableVertexAttribArray(index);
			if (attr.UploadAsInteger)
				GL.VertexAttribIPointer(index, attr.PointerCount, GetIntPointerType(attr.PointerType), structSize, offset);
			else
				GL.VertexAttribPointer(index, attr.PointerCount, attr.PointerType, attr.Normalized, structSize, offset);
		}
	}

	private static VertexAttribIType GetIntPointerType(VertexAttribPointerType attrPointerType)
	{
		return attrPointerType switch
		{
			VertexAttribPointerType.Byte => VertexAttribIType.Byte,
			VertexAttribPointerType.UnsignedByte => VertexAttribIType.UnsignedByte,
			VertexAttribPointerType.Short => VertexAttribIType.Short,
			VertexAttribPointerType.UnsignedShort => VertexAttribIType.UnsignedShort,
			VertexAttribPointerType.Int => VertexAttribIType.Int,
			VertexAttribPointerType.UnsignedInt => VertexAttribIType.UnsignedInt,
			_ => throw new ArgumentOutOfRangeException(nameof(attrPointerType), attrPointerType, null)
		};
	}
}