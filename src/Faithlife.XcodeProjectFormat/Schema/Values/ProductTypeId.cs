using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A product type identifier (for example, <c>com.apple.product-type.application</c>).
/// </summary>
public sealed record ProductTypeId(string Id) : IXCJsonStringCodable<ProductTypeId>, IComparable<ProductTypeId>
{
	public const string AbbreviatablePrefix = "com.apple.product-type.";

	/// <summary>
	/// Creates a product type identifier from its abbreviated representation (for example, <c>application</c>).
	/// </summary>
	public static ProductTypeId FromAbbreviatedRepresentation(string abbreviatedRepresentation) => new(AbbreviatablePrefix + abbreviatedRepresentation);

	public string Id { get; init; } = Id ?? throw new ArgumentNullException(nameof(Id));

	/// <summary>
	/// The identifier without the <c>com.apple.product-type.</c> prefix, or null if it doesn't have that prefix.
	/// </summary>
	public string? AbbreviatedRepresentation => Id.StartsWith(AbbreviatablePrefix, StringComparison.Ordinal) ? Id[AbbreviatablePrefix.Length..] : null;

	public int CompareTo(ProductTypeId? other) => XCJsonCoding.CompareStrings(Id, other?.Id);

	public static bool operator <(ProductTypeId? left, ProductTypeId? right) => Comparer<ProductTypeId>.Default.Compare(left, right) < 0;

	public static bool operator <=(ProductTypeId? left, ProductTypeId? right) => Comparer<ProductTypeId>.Default.Compare(left, right) <= 0;

	public static bool operator >(ProductTypeId? left, ProductTypeId? right) => Comparer<ProductTypeId>.Default.Compare(left, right) > 0;

	public static bool operator >=(ProductTypeId? left, ProductTypeId? right) => Comparer<ProductTypeId>.Default.Compare(left, right) >= 0;

	public override string ToString() => Id;

	string IXCJsonStringEncodable.EncodableStringRepresentation => Id;

	static ProductTypeId IXCJsonStringCodable<ProductTypeId>.FromEncodableStringRepresentation(string value) => new(value);
}
