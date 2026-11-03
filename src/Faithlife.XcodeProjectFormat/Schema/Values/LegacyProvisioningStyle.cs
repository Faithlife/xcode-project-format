using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The legacy code signing provisioning style of a target.
/// </summary>
public enum LegacyProvisioningStyle
{
	[XCJsonName("automatic")]
	Automatic,

	[XCJsonName("manual")]
	Manual,
}
