using OpenTK.Graphics.OpenGL;

namespace RogueRender;

public class BufferAttributeAttribute(int pointerCount, VertexAttribPointerType pointerType, bool normalize = false, bool uploadAsInteger = false) : Attribute
{
	public int PointerCount { get; } = pointerCount;
	public VertexAttribPointerType PointerType { get; } = pointerType;
	public bool Normalized { get; set; } = normalize;
	public bool UploadAsInteger { get; } = uploadAsInteger;
}