namespace Faithlife.XcodeProjectFormat.Serialization;

internal sealed record XCJsonFieldOrComment
{
	public XCJsonFieldOrComment(XCJsonField field) => Field = field;

	public XCJsonFieldOrComment(XCJsonComment comment) => Comment = comment;

	public XCJsonFieldOrComment(string key, XCJsonValue value) => Field = new XCJsonField(key, value);

	public XCJsonField? Field { get; }

	public XCJsonComment? Comment { get; }

	public bool IsField => Field is not null;

	public static implicit operator XCJsonFieldOrComment(XCJsonField field) => new(field);

	public static implicit operator XCJsonFieldOrComment(XCJsonComment comment) => new(comment);
}
