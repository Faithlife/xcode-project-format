namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Specifies the JSON spelling of an enum member.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
internal sealed class XCJsonNameAttribute(string name) : Attribute
{
	public string Name { get; } = name;
}
