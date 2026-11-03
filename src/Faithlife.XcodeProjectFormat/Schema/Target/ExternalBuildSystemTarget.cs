using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A target that is built by running an external build tool.
/// </summary>
public sealed class ExternalBuildSystemTarget : Target
{
	public ExternalBuildSystemTarget(string name, ObjectId objectId, string buildToolPath)
		: base(name, objectId, TargetKind.ExternalBuildSystem)
	{
		BuildToolPath = buildToolPath ?? throw new ArgumentNullException(nameof(buildToolPath));
	}

	public string BuildToolPath { get; set; }

	public string BuildToolArguments { get; set; } = "";

	public string? BuildToolWorkingDirectory { get; set; }

	public bool PassBuildSettingsInEnvironment { get; set; } = true;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		base.EncodeFields(container);
		container.Encode("build-tool-path", BuildToolPath);
		container.EncodeIfNotDefault("build-tool-arguments", BuildToolArguments, "");
		container.EncodeIfNotNull("build-tool-working-directory", BuildToolWorkingDirectory);
		container.EncodeIfNotDefault("pass-build-settings-in-environment", PassBuildSettingsInEnvironment, true);
	}

	internal static ExternalBuildSystemTarget DecodeFields(XCJsonDecoder.KeyedContainer container, string name, ObjectId objectId)
	{
		var target = new ExternalBuildSystemTarget(name, objectId, "");
		target.DecodeFields(container);
		target.BuildToolPath = container.Decode<string>("build-tool-path");
		target.BuildToolArguments = container.Decode("build-tool-arguments", "");
		target.BuildToolWorkingDirectory = container.DecodeOptional<string>("build-tool-working-directory");
		target.PassBuildSettingsInEnvironment = container.Decode("pass-build-settings-in-environment", true);
		return target;
	}
}
