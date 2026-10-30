using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A Swift package in a remote repository.
/// </summary>
public sealed class RemoteSwiftPackage : SwiftPackageLocation
{
	public RemoteSwiftPackage(string repositoryUrl) => RepositoryUrl = repositoryUrl ?? throw new ArgumentNullException(nameof(repositoryUrl));

	public string RepositoryUrl { get; set; }

	public SwiftPackageVersionConstraint? VersionConstraint { get; set; }

	internal override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.Encode("kind", "remote");
		container.Encode("repository", RepositoryUrl);
		container.EncodeIfNotNull("version", VersionConstraint);
	}
}
