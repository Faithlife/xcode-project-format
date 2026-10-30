namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Copy Headers" build phase.
/// </summary>
public sealed class HeadersBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Headers;
}
