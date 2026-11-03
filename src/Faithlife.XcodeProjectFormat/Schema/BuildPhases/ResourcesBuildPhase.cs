namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Copy Bundle Resources" build phase.
/// </summary>
public sealed class ResourcesBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Resources;
}
