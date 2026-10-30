using System.Text;

namespace Faithlife.XcodeProjectFormat.Serialization;

internal enum XCJsonValueType
{
	Null,
	Boolean,
	Integer,
	Double,
	String,
	Array,
	Object,
}

internal enum XCJsonCommentStyle
{
	Line,
	Block,
}

internal enum XCJsonPrintingDensity
{
	Compact,
	Sprawling,
}

/// <summary>
/// An immutable JSON value that preserves object field order and comments.
/// </summary>
internal sealed class XCJsonValue : IEquatable<XCJsonValue>
{
	public static readonly XCJsonValue Null = new(XCJsonValueType.Null, null);
	public static readonly XCJsonValue True = new(XCJsonValueType.Boolean, true);
	public static readonly XCJsonValue False = new(XCJsonValueType.Boolean, false);

	public static XCJsonValue FromBoolean(bool value) => value ? True : False;

	public static XCJsonValue FromInteger(long value) => new(XCJsonValueType.Integer, value);

	public static XCJsonValue FromDouble(double value) => new(XCJsonValueType.Double, value);

	public static XCJsonValue FromString(string value) => new(XCJsonValueType.String, value ?? throw new ArgumentNullException(nameof(value)));

	public static XCJsonValue FromArray(IEnumerable<XCJsonValueOrComment> entries) => new(XCJsonValueType.Array, entries.ToList());

	public static XCJsonValue FromArray(params XCJsonValueOrComment[] entries) => FromArray((IEnumerable<XCJsonValueOrComment>) entries);

	public static XCJsonValue FromObject(IEnumerable<XCJsonFieldOrComment> entries) => new(XCJsonValueType.Object, entries.ToList());

	public static XCJsonValue FromObject(params XCJsonFieldOrComment[] entries) => FromObject((IEnumerable<XCJsonFieldOrComment>) entries);

	public static implicit operator XCJsonValue(bool value) => FromBoolean(value);

	public static implicit operator XCJsonValue(long value) => FromInteger(value);

	public static implicit operator XCJsonValue(double value) => FromDouble(value);

	public static implicit operator XCJsonValue(string value) => FromString(value);

	public XCJsonValueType Type { get; }

	public bool BooleanValue => Type == XCJsonValueType.Boolean ? (bool) m_payload! : throw new InvalidOperationException();

	public long IntegerValue => Type == XCJsonValueType.Integer ? (long) m_payload! : throw new InvalidOperationException();

	public double DoubleValue => Type == XCJsonValueType.Double ? (double) m_payload! : throw new InvalidOperationException();

	public string StringValue => Type == XCJsonValueType.String ? (string) m_payload! : throw new InvalidOperationException();

	public IReadOnlyList<XCJsonValueOrComment> ArrayEntries => Type == XCJsonValueType.Array ? (IReadOnlyList<XCJsonValueOrComment>) m_payload! : throw new InvalidOperationException();

	public IReadOnlyList<XCJsonFieldOrComment> ObjectEntries => Type == XCJsonValueType.Object ? (IReadOnlyList<XCJsonFieldOrComment>) m_payload! : throw new InvalidOperationException();

	public bool IsContainer => Type is XCJsonValueType.Array or XCJsonValueType.Object;

	/// <summary>
	/// Parses JSON5 text. Comments in the input are discarded.
	/// </summary>
	public static XCJsonValue Parse(string text) => XCJsonParser.Parse(text);

	public string ToText(XCJsonEncodingOptions? options = null, IReadOnlyDictionary<XCJsonPath, XCJsonPrintingDensity>? densities = null) =>
		Encoding.UTF8.GetString(ToUtf8(options, densities));

	public byte[] ToUtf8(XCJsonEncodingOptions? options = null, IReadOnlyDictionary<XCJsonPath, XCJsonPrintingDensity>? densities = null) =>
		XCJsonPrinter.PrintUtf8(this, options ?? XCJsonEncodingOptions.Default, densities);

	public bool Equals(XCJsonValue? other)
	{
		if (ReferenceEquals(this, other))
			return true;
		if (other is null || other.Type != Type)
			return false;

		return Type switch
		{
			XCJsonValueType.Null => true,
			XCJsonValueType.Array => ArrayEntries.SequenceEqual(other.ArrayEntries),
			XCJsonValueType.Object => ObjectEntries.SequenceEqual(other.ObjectEntries),
			_ => m_payload!.Equals(other.m_payload),
		};
	}

	public override bool Equals(object? obj) => obj is XCJsonValue other && Equals(other);

	public override int GetHashCode()
	{
		var hash = default(HashCode);
		hash.Add(Type);
		switch (Type)
		{
			case XCJsonValueType.Null:
				break;
			case XCJsonValueType.Array:
				foreach (var entry in ArrayEntries)
					hash.Add(entry);
				break;
			case XCJsonValueType.Object:
				foreach (var entry in ObjectEntries)
					hash.Add(entry);
				break;
			default:
				hash.Add(m_payload);
				break;
		}
		return hash.ToHashCode();
	}

	public override string ToString() => ToText(XCJsonEncodingOptions.Default with { AddTrailingNewline = false });

	internal static string GetErrorMessageName(XCJsonValueType type) => type switch
	{
		XCJsonValueType.Null => "null",
		XCJsonValueType.Boolean => "boolean",
		XCJsonValueType.Integer => "integer",
		XCJsonValueType.Double => "double",
		XCJsonValueType.String => "string",
		XCJsonValueType.Array => "array",
		XCJsonValueType.Object => "dictionary",
		_ => throw new ArgumentOutOfRangeException(nameof(type)),
	};

	private XCJsonValue(XCJsonValueType type, object? payload)
	{
		Type = type;
		m_payload = payload;
	}

	private readonly object? m_payload;
}
