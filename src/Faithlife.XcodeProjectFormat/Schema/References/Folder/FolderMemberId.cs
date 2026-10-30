using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The relative path of a member of a folder reference.
/// </summary>
public sealed record FolderMemberId(string Value) : IXCJsonStringCodable<FolderMemberId>, IComparable<FolderMemberId>
{
	public string Value { get; init; } = Value ?? throw new ArgumentNullException(nameof(Value));

	public int CompareTo(FolderMemberId? other) => XCJsonCoding.CompareStrings(Value, other?.Value);

	public static bool operator <(FolderMemberId? left, FolderMemberId? right) => Comparer<FolderMemberId>.Default.Compare(left, right) < 0;

	public static bool operator <=(FolderMemberId? left, FolderMemberId? right) => Comparer<FolderMemberId>.Default.Compare(left, right) <= 0;

	public static bool operator >(FolderMemberId? left, FolderMemberId? right) => Comparer<FolderMemberId>.Default.Compare(left, right) > 0;

	public static bool operator >=(FolderMemberId? left, FolderMemberId? right) => Comparer<FolderMemberId>.Default.Compare(left, right) >= 0;

	public override string ToString() => Value;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Value;

	static FolderMemberId IXCJsonStringCodable<FolderMemberId>.FromEncodableStringRepresentation(string value) => new(value);
}
