namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Build Carbon Resources" (Rez) build phase.
/// </summary>
public sealed class RezBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Rez;
}
