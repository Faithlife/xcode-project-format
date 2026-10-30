using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The identifier of an object in a project.
/// </summary>
public sealed record ObjectId(string Value) : IXCJsonStringCodable<ObjectId>, IComparable<ObjectId>
{
	public string Value { get; init; } = Value ?? throw new ArgumentNullException(nameof(Value));

	public int CompareTo(ObjectId? other) => XCJsonCoding.CompareStrings(Value, other?.Value);

	public static bool operator <(ObjectId? left, ObjectId? right) => Comparer<ObjectId>.Default.Compare(left, right) < 0;

	public static bool operator <=(ObjectId? left, ObjectId? right) => Comparer<ObjectId>.Default.Compare(left, right) <= 0;

	public static bool operator >(ObjectId? left, ObjectId? right) => Comparer<ObjectId>.Default.Compare(left, right) > 0;

	public static bool operator >=(ObjectId? left, ObjectId? right) => Comparer<ObjectId>.Default.Compare(left, right) >= 0;

	public override string ToString() => Value;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Value;

	static ObjectId IXCJsonStringCodable<ObjectId>.FromEncodableStringRepresentation(string value) => new(value);
}
