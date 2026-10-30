using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Copy Files" build phase.
/// </summary>
public sealed class CopyFilesBuildPhase : BuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Copy;

	public BundleBasePath? BundleBasePath { get; set; }

	/// <summary>
	/// The destination path, relative to <see cref="BundleBasePath"/>.
	/// </summary>
	public string RelativePath { get; set; } = "";

	public BuildPhaseScope Scope { get; set; }

	private protected override void EncodeProperties(XCJsonEncoder.KeyedContainer container)
	{
		base.EncodeProperties(container);
		container.EncodeIfNotNull("bundle-base-path", BundleBasePath);
		container.EncodeIfNotDefault("relative-path", RelativePath, "");
		container.EncodeIfNotDefault("scope", Scope, BuildPhaseScope.Always);
	}

	private protected override void DecodeProperties(XCJsonDecoder.KeyedContainer container)
	{
		base.DecodeProperties(container);
		BundleBasePath = container.DecodeOptionalValue<BundleBasePath>("bundle-base-path");
		RelativePath = container.Decode("relative-path", "");
		Scope = container.Decode("scope", BuildPhaseScope.Always);
	}
}
