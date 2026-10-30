using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// An on-demand resource asset tag.
/// </summary>
public sealed record AssetTag(string Name) : IXCJsonStringCodable<AssetTag>, IComparable<AssetTag>
{
	public string Name { get; init; } = Name ?? throw new ArgumentNullException(nameof(Name));

	public int CompareTo(AssetTag? other) => XCJsonCoding.CompareStrings(Name, other?.Name);

	public static bool operator <(AssetTag? left, AssetTag? right) => Comparer<AssetTag>.Default.Compare(left, right) < 0;

	public static bool operator <=(AssetTag? left, AssetTag? right) => Comparer<AssetTag>.Default.Compare(left, right) <= 0;

	public static bool operator >(AssetTag? left, AssetTag? right) => Comparer<AssetTag>.Default.Compare(left, right) > 0;

	public static bool operator >=(AssetTag? left, AssetTag? right) => Comparer<AssetTag>.Default.Compare(left, right) >= 0;

	public override string ToString() => Name;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Name;

	static AssetTag IXCJsonStringCodable<AssetTag>.FromEncodableStringRepresentation(string value) => new(value);
}
