using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The name of a build configuration (for example, <c>Debug</c>).
/// </summary>
public sealed record ConfigurationName(string Name) : IXCJsonStringCodable<ConfigurationName>, IComparable<ConfigurationName>
{
	public string Name { get; init; } = Name ?? throw new ArgumentNullException(nameof(Name));

	public int CompareTo(ConfigurationName? other) => XCJsonCoding.CompareStrings(Name, other?.Name);

	public static bool operator <(ConfigurationName? left, ConfigurationName? right) => Comparer<ConfigurationName>.Default.Compare(left, right) < 0;

	public static bool operator <=(ConfigurationName? left, ConfigurationName? right) => Comparer<ConfigurationName>.Default.Compare(left, right) <= 0;

	public static bool operator >(ConfigurationName? left, ConfigurationName? right) => Comparer<ConfigurationName>.Default.Compare(left, right) > 0;

	public static bool operator >=(ConfigurationName? left, ConfigurationName? right) => Comparer<ConfigurationName>.Default.Compare(left, right) >= 0;

	public override string ToString() => Name;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Name;

	static ConfigurationName IXCJsonStringCodable<ConfigurationName>.FromEncodableStringRepresentation(string value) => new(value);
}
