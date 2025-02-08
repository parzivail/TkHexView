using System.Reflection;

namespace RogueRender;

public class BufferUtil
{
	public static IEnumerable<(uint Index, FieldInfo Field, BufferAttributeAttribute BufferAttribute)> GetBufferAttributes(Type type)
	{
		var i = 0u;
		foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
		{
			if (fieldInfo.GetCustomAttribute<BufferAttributeAttribute>() is not { } bufferAttribute)
				throw new NotSupportedException($"All fields in a struct must have a [{nameof(BufferAttributeAttribute)}]");

			yield return (i, fieldInfo, bufferAttribute);

			i++;
		}
	}
}