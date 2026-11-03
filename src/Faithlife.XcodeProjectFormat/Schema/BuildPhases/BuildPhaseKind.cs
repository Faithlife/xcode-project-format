using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The kind of a build phase.
/// </summary>
public enum BuildPhaseKind
{
	[XCJsonName("apple-script")]
	AppleScript,

	[XCJsonName("frameworks")]
	Frameworks,

	[XCJsonName("headers")]
	Headers,

	[XCJsonName("java-archive")]
	JavaArchive,

	[XCJsonName("resources")]
	Resources,

	[XCJsonName("rez")]
	Rez,

	[XCJsonName("compile-sources")]
	Sources,

	[XCJsonName("copy")]
	Copy,

	[XCJsonName("script")]
	Script,
}
