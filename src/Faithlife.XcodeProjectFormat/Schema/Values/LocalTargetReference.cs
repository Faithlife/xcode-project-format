using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to a target in the same project, by name.
/// </summary>
public sealed record LocalTargetReference(string TargetName) : IXCJsonStringCodable<LocalTargetReference>, IComparable<LocalTargetReference>
{
	public string TargetName { get; init; } = TargetName ?? throw new ArgumentNullException(nameof(TargetName));

	public int CompareTo(LocalTargetReference? other) => XCJsonCoding.CompareStrings(TargetName, other?.TargetName);

	public static bool operator <(LocalTargetReference? left, LocalTargetReference? right) => Comparer<LocalTargetReference>.Default.Compare(left, right) < 0;

	public static bool operator <=(LocalTargetReference? left, LocalTargetReference? right) => Comparer<LocalTargetReference>.Default.Compare(left, right) <= 0;

	public static bool operator >(LocalTargetReference? left, LocalTargetReference? right) => Comparer<LocalTargetReference>.Default.Compare(left, right) > 0;

	public static bool operator >=(LocalTargetReference? left, LocalTargetReference? right) => Comparer<LocalTargetReference>.Default.Compare(left, right) >= 0;

	public override string ToString() => TargetName;

	string IXCJsonStringEncodable.EncodableStringRepresentation => TargetName;

	static LocalTargetReference IXCJsonStringCodable<LocalTargetReference>.FromEncodableStringRepresentation(string value) => new(value);
}
