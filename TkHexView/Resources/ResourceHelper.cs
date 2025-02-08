using System.Reflection;

namespace RogueRender.Resources;

public static class ResourceHelper
{
	public static Stream Get(string filename)
	{
		var assembly = Assembly.GetExecutingAssembly();
		return assembly.GetManifestResourceStream($"{typeof(ResourceHelper).Namespace}.{filename}");
	}

	public static string Read(string filename)
	{
		using var stream = new StreamReader(Get(filename));
		return stream.ReadToEnd();
	}
}