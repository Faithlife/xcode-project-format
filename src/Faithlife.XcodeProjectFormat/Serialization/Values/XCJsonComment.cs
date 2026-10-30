namespace Faithlife.XcodeProjectFormat.Serialization;

internal sealed record XCJsonComment
{
	public XCJsonComment(XCJsonCommentStyle style, string content)
	{
		if (style == XCJsonCommentStyle.Block && (content.Contains("/*", StringComparison.Ordinal) || content.Contains("*/", StringComparison.Ordinal)))
			throw new XcodeProjectFormatException("Comment contains internal terminators");

		Style = style;
		Content = content;
	}

	public XCJsonCommentStyle Style { get; }

	public string Content { get; }

	public bool AllowsCompactPrinting => Style == XCJsonCommentStyle.Block && !ContainsLineSeparator(Content);

	internal static bool IsJsonLineSeparator(char ch) => ch is '\n' or '\r' or '\u2028' or '\u2029';

	// like Swift (which compares grapheme clusters), "\r\n" is not considered a line separator
	private static bool ContainsLineSeparator(string content)
	{
		for (var index = 0; index < content.Length; index++)
		{
			if (content[index] == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
				index++;
			else if (IsJsonLineSeparator(content[index]))
				return true;
		}
		return false;
	}
}
