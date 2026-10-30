namespace Faithlife.XcodeProjectFormat.Serialization;

internal sealed record XCJsonValueOrComment
{
	public XCJsonValueOrComment(XCJsonValue value) => Value = value;

	public XCJsonValueOrComment(XCJsonComment comment) => Comment = comment;

	public XCJsonValue? Value { get; }

	public XCJsonComment? Comment { get; }

	public bool IsValue => Value is not null;

	public static implicit operator XCJsonValueOrComment(XCJsonValue value) => new(value);

	public static implicit operator XCJsonValueOrComment(XCJsonComment comment) => new(comment);

	public static implicit operator XCJsonValueOrComment(bool value) => new(XCJsonValue.FromBoolean(value));

	public static implicit operator XCJsonValueOrComment(long value) => new(XCJsonValue.FromInteger(value));

	public static implicit operator XCJsonValueOrComment(double value) => new(XCJsonValue.FromDouble(value));

	public static implicit operator XCJsonValueOrComment(string value) => new(XCJsonValue.FromString(value));
}
