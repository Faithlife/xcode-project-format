using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The line ending used by a file.
/// </summary>
public enum LineEnding
{
	[XCJsonName("line-feed")]
	LineFeed,

	[XCJsonName("carriage-return")]
	CarriageReturn,

	[XCJsonName("carriage-return-line-feed")]
	CarriageReturnLineFeed,

	[XCJsonName("preserve")]
	Preserve,
}
