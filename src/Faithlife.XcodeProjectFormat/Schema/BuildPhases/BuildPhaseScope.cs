using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// When a build phase runs.
/// </summary>
public enum BuildPhaseScope
{
	[XCJsonName("always")]
	Always,

	[XCJsonName("install")]
	Install,
}
