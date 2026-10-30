using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A component of a <see cref="NamePath"/>: either a relative reference (<c>.</c> or <c>..</c>) or the name of a child.
/// </summary>
public sealed record NamePathComponent : IXCJsonEncodable, IXCJsonDecodable<NamePathComponent>
{
	public static NamePathComponent Child(string name) => new(name ?? throw new ArgumentNullException(nameof(name)), null);

	public static NamePathComponent Relative(NamePathRelativeReference reference) => new(null, reference);

	public static readonly NamePathComponent Current = Relative(NamePathRelativeReference.Current);

	public static readonly NamePathComponent Parent = Relative(NamePathRelativeReference.Parent);

	/// <summary>
	/// The name of the child, or null if this is a relative reference.
	/// </summary>
	public string? ChildName { get; }

	/// <summary>
	/// The relative reference, or null if this is a child name.
	/// </summary>
	public NamePathRelativeReference? RelativeReference { get; }

	public override string ToString() => ChildName ?? GetRelativeReferenceString(RelativeReference!.Value);

	internal string? LosslessStringEncoding => ChildName is null ? ToString() : CanLosslesslyEncode(ChildName) ? ChildName : null;

	internal static NamePathComponent ParseLosslessStringEncoding(string value, bool requiresSlashValidation = true)
	{
		if (TryParseRelativeReference(value) is { } relative)
			return Relative(relative);
		if (!CanLosslesslyEncode(value, requiresSlashValidation, requiresRelativeReferenceValidation: false))
			throw new XcodeProjectFormatException($"The value {value.SmartQuoted()} isn't losslessly representable as a group path component");
		return Child(value);
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (ChildName is null)
			encoder.OpenPrimitiveContainer().Encode(ToString());
		else if (CanLosslesslyEncode(ChildName))
			encoder.OpenPrimitiveContainer().Encode(ChildName);
		else
			encoder.OpenKeyedContainer().Encode("name", ChildName);
	}

	static NamePathComponent IXCJsonDecodable<NamePathComponent>.Decode(XCJsonDecoder decoder) =>
		decoder.CurrentNodeType == XCJsonValueType.String ? ParseLosslessStringEncoding(decoder.DecodePrimitiveString()) : Child(decoder.OpenKeyedContainer().Decode<string>("name"));

	private NamePathComponent(string? childName, NamePathRelativeReference? relativeReference)
	{
		ChildName = childName;
		RelativeReference = relativeReference;
	}

	private static bool CanLosslesslyEncode(string name, bool requiresSlashValidation = true, bool requiresRelativeReferenceValidation = true) =>
		(!requiresSlashValidation || !name.Contains('/', StringComparison.Ordinal)) && (!requiresRelativeReferenceValidation || TryParseRelativeReference(name) is null);

	private static NamePathRelativeReference? TryParseRelativeReference(string value) => value switch
	{
		"." => NamePathRelativeReference.Current,
		".." => NamePathRelativeReference.Parent,
		_ => null,
	};

	private static string GetRelativeReferenceString(NamePathRelativeReference reference) => reference == NamePathRelativeReference.Current ? "." : "..";
}
