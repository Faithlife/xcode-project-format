using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The name of a Swift package.
/// </summary>
public sealed record SwiftPackageName(string PackageName) : IXCJsonStringCodable<SwiftPackageName>, IComparable<SwiftPackageName>
{
	public string PackageName { get; init; } = PackageName ?? throw new ArgumentNullException(nameof(PackageName));

	public int CompareTo(SwiftPackageName? other) => XCJsonCoding.CompareStrings(PackageName, other?.PackageName);

	public static bool operator <(SwiftPackageName? left, SwiftPackageName? right) => Comparer<SwiftPackageName>.Default.Compare(left, right) < 0;

	public static bool operator <=(SwiftPackageName? left, SwiftPackageName? right) => Comparer<SwiftPackageName>.Default.Compare(left, right) <= 0;

	public static bool operator >(SwiftPackageName? left, SwiftPackageName? right) => Comparer<SwiftPackageName>.Default.Compare(left, right) > 0;

	public static bool operator >=(SwiftPackageName? left, SwiftPackageName? right) => Comparer<SwiftPackageName>.Default.Compare(left, right) >= 0;

	public override string ToString() => PackageName;

	string IXCJsonStringEncodable.EncodableStringRepresentation => PackageName;

	static SwiftPackageName IXCJsonStringCodable<SwiftPackageName>.FromEncodableStringRepresentation(string value) => new(value);
}
