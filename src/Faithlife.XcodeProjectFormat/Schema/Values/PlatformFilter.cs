using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A platform filter (for example, <c>ios</c> or <c>macos</c>).
/// </summary>
public sealed record PlatformFilter(string PlatformId) : IXCJsonStringCodable<PlatformFilter>, IComparable<PlatformFilter>
{
	public string PlatformId { get; init; } = PlatformId ?? throw new ArgumentNullException(nameof(PlatformId));

	public int CompareTo(PlatformFilter? other) => XCJsonCoding.CompareStrings(PlatformId, other?.PlatformId);

	public static bool operator <(PlatformFilter? left, PlatformFilter? right) => Comparer<PlatformFilter>.Default.Compare(left, right) < 0;

	public static bool operator <=(PlatformFilter? left, PlatformFilter? right) => Comparer<PlatformFilter>.Default.Compare(left, right) <= 0;

	public static bool operator >(PlatformFilter? left, PlatformFilter? right) => Comparer<PlatformFilter>.Default.Compare(left, right) > 0;

	public static bool operator >=(PlatformFilter? left, PlatformFilter? right) => Comparer<PlatformFilter>.Default.Compare(left, right) >= 0;

	public override string ToString() => PlatformId;

	string IXCJsonStringEncodable.EncodableStringRepresentation => PlatformId;

	static PlatformFilter IXCJsonStringCodable<PlatformFilter>.FromEncodableStringRepresentation(string value) => new(value);
}
