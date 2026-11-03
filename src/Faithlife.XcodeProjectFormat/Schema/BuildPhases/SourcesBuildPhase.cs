namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Compile Sources" build phase.
/// </summary>
public sealed class SourcesBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Sources;
}
