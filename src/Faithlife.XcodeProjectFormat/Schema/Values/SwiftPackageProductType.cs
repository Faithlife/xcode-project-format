using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The type of a Swift package product.
/// </summary>
public enum SwiftPackageProductType
{
	[XCJsonName("other")]
	Other,

	[XCJsonName("build-tool-plugin")]
	BuildToolPlugin,
}
