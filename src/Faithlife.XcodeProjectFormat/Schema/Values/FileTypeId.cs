using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A file type identifier (for example, <c>sourcecode.swift</c>).
/// </summary>
public sealed record FileTypeId(string Id) : IXCJsonStringCodable<FileTypeId>, IComparable<FileTypeId>
{
	public string Id { get; init; } = Id ?? throw new ArgumentNullException(nameof(Id));

	public int CompareTo(FileTypeId? other) => XCJsonCoding.CompareStrings(Id, other?.Id);

	public static bool operator <(FileTypeId? left, FileTypeId? right) => Comparer<FileTypeId>.Default.Compare(left, right) < 0;

	public static bool operator <=(FileTypeId? left, FileTypeId? right) => Comparer<FileTypeId>.Default.Compare(left, right) <= 0;

	public static bool operator >(FileTypeId? left, FileTypeId? right) => Comparer<FileTypeId>.Default.Compare(left, right) > 0;

	public static bool operator >=(FileTypeId? left, FileTypeId? right) => Comparer<FileTypeId>.Default.Compare(left, right) >= 0;

	public override string ToString() => Id;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Id;

	static FileTypeId IXCJsonStringCodable<FileTypeId>.FromEncodableStringRepresentation(string value) => new(value);
}
