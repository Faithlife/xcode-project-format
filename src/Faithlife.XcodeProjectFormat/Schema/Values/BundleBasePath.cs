using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The base location for a copy files build phase.
/// </summary>
public enum BundleBasePath
{
	[XCJsonName("root")]
	Root,

	[XCJsonName("build-products-directory")]
	ProductsDirectory,

	[XCJsonName("shared-frameworks-directory")]
	SharedFrameworksDirectory,

	[XCJsonName("shared-support-directory")]
	SharedSupportDirectory,

	[XCJsonName("java-directory")]
	JavaDirectory,

	[XCJsonName("frameworks-directory")]
	FrameworksDirectory,

	[XCJsonName("resources-directory")]
	ResourcesDirectory,

	[XCJsonName("package-info-file")]
	PackageInfo,

	[XCJsonName("apple-scripts-directory")]
	AppleScriptsDirectory,

	[XCJsonName("plugins-directory")]
	PlugInsDirectory,

	[XCJsonName("private-headers-directory")]
	PrivateHeadersDirectory,

	[XCJsonName("headers-directory")]
	HeadersDirectory,

	[XCJsonName("contents-directory")]
	ContentsDirectory,

	[XCJsonName("executables-directory")]
	ExecutablesDirectory,

	[XCJsonName("info-plist-file")]
	InfoPlist,

	[XCJsonName("main-executable-file")]
	MainExecutable,

	[XCJsonName("shallow-main-executable-file")]
	MainExecutableShallow,
}
