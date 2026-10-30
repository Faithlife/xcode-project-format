using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A Swift package on the local file system.
/// </summary>
public sealed class LocalSwiftPackage : SwiftPackageLocation
{
	public LocalSwiftPackage(string path) => Path = path ?? throw new ArgumentNullException(nameof(path));

	public string Path { get; set; }

	internal override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.Encode("kind", "local");
		container.Encode("path", Path);
	}
}
