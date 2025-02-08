using OpenTK.Graphics.OpenGL;

namespace RogueRender;

public class ShaderProgram
{
	private readonly Dictionary<string, int> _uniformLocations = new();

	public int Handle { get; init; } = GL.CreateProgram();

	public void AttachShader(ShaderType shaderType, string source)
	{
		var shader = GL.CreateShader(shaderType);
		GL.ShaderSource(shader, source);
		GL.CompileShader(shader);

		var compiled = -1;
		GL.GetShaderi(shader, ShaderParameterName.CompileStatus, ref compiled);
		if (compiled < 1)
		{
			GL.GetShaderInfoLog(shader, out var info);
			throw new InvalidDataException(info);
		}

		GL.AttachShader(Handle, shader);
	}

	public void Link()
	{
		GL.LinkProgram(Handle);

		var compiled = -1;
		GL.GetProgrami(Handle, ProgramProperty.LinkStatus, ref compiled);
		if (compiled < 1)
		{
			GL.GetProgramInfoLog(Handle, out var info);
			throw new InvalidDataException(info);
		}
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}

	public void BindAttribLocation(uint index, string name)
	{
		GL.BindAttribLocation(Handle, index, name);
	}

	public void BindStructAttribLocations<T>(Func<string, string?> shaderIdentifierMapper) where T : struct
	{
		foreach (var (index, field, attr) in BufferUtil.GetBufferAttributes(typeof(T)))
		{
			var mappedName = shaderIdentifierMapper.Invoke(field.Name);
			if (mappedName == null)
				continue;

			BindAttribLocation(index, mappedName);
		}
	}

	public int GetUniform(string name)
	{
		if (_uniformLocations.TryGetValue(name, out var location))
			return location;

		location = GL.GetUniformLocation(Handle, name);
		_uniformLocations[name] = location;
		return location;
	}
}