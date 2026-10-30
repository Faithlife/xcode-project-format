using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Run Script" build phase.
/// </summary>
public sealed class ScriptBuildPhase : BuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Script;

	public string ShellPath { get; set; } = "/bin/sh";

	public string Script { get; set; } = "";

	public bool LogEnvironmentVariables { get; set; }

	public List<string> InputPaths { get; set; } = [];

	public List<string> InputFileListPaths { get; set; } = [];

	public List<string> OutputPaths { get; set; } = [];

	public List<string> OutputFileListPaths { get; set; } = [];

	public string? DependencyFile { get; set; }

	public bool RunOnEveryBuild { get; set; }

	public BuildPhaseScope Scope { get; set; }

	private protected override void EncodeProperties(XCJsonEncoder.KeyedContainer container)
	{
		base.EncodeProperties(container);
		container.EncodeIfNotDefault("log-environment-variables", LogEnvironmentVariables, false);
		container.EncodeIfNotEmpty("input-paths", InputPaths);
		container.EncodeIfNotEmpty("input-file-list-paths", InputFileListPaths);
		container.EncodeIfNotEmpty("output-paths", OutputPaths);
		container.EncodeIfNotEmpty("output-file-list-paths", OutputFileListPaths);
		container.EncodeIfNotNull("dependency-file", DependencyFile);
		container.EncodeIfNotDefault("run-on-every-build", RunOnEveryBuild, false);
		container.EncodeIfNotDefault("scope", Scope, BuildPhaseScope.Always);
		container.Encode("shell", ShellPath);
		container.Encode("script", new MultilineText(Script));
	}

	private protected override void DecodeProperties(XCJsonDecoder.KeyedContainer container)
	{
		base.DecodeProperties(container);
		LogEnvironmentVariables = container.Decode("log-environment-variables", false);
		InputPaths = container.Decode("input-paths", new List<string>());
		InputFileListPaths = container.Decode("input-file-list-paths", new List<string>());
		OutputPaths = container.Decode("output-paths", new List<string>());
		OutputFileListPaths = container.Decode("output-file-list-paths", new List<string>());
		DependencyFile = container.DecodeOptional<string>("dependency-file");
		RunOnEveryBuild = container.Decode("run-on-every-build", false);
		Scope = container.Decode("scope", BuildPhaseScope.Always);
		ShellPath = container.Decode<string>("shell");
		Script = container.Decode<MultilineText>("script").Text;
	}
}
