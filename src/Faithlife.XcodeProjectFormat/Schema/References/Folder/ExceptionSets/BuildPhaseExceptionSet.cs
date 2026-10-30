using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// Exceptions to the membership of a folder's contents in a build phase.
/// </summary>
public sealed class BuildPhaseExceptionSet : FolderExceptionSet
{
	public BuildPhaseExceptionSet(ProjectBuildPhaseReference buildPhase) => BuildPhase = buildPhase ?? throw new ArgumentNullException(nameof(buildPhase));

	public ProjectBuildPhaseReference BuildPhase { get; set; }

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.Encode("build-phase", BuildPhase);
		EncodeCommonFields(container);
	}

	internal static BuildPhaseExceptionSet DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var exceptionSet = new BuildPhaseExceptionSet(container.Decode<ProjectBuildPhaseReference>("build-phase"));
		exceptionSet.DecodeCommonFields(container);
		return exceptionSet;
	}
}
