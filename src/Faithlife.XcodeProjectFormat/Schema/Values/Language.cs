using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A localization language identifier (for example, <c>en</c> or <c>Base</c>).
/// </summary>
public sealed record Language(string LanguageId) : IXCJsonStringCodable<Language>, IComparable<Language>
{
	public string LanguageId { get; init; } = LanguageId ?? throw new ArgumentNullException(nameof(LanguageId));

	public int CompareTo(Language? other) => XCJsonCoding.CompareStrings(LanguageId, other?.LanguageId);

	public static bool operator <(Language? left, Language? right) => Comparer<Language>.Default.Compare(left, right) < 0;

	public static bool operator <=(Language? left, Language? right) => Comparer<Language>.Default.Compare(left, right) <= 0;

	public static bool operator >(Language? left, Language? right) => Comparer<Language>.Default.Compare(left, right) > 0;

	public static bool operator >=(Language? left, Language? right) => Comparer<Language>.Default.Compare(left, right) >= 0;

	public override string ToString() => LanguageId;

	string IXCJsonStringEncodable.EncodableStringRepresentation => LanguageId;

	static Language IXCJsonStringCodable<Language>.FromEncodableStringRepresentation(string value) => new(value);
}
