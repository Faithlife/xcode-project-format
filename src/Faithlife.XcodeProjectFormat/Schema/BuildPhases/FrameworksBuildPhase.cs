namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Link Binary With Libraries" build phase.
/// </summary>
public sealed class FrameworksBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.Frameworks;
}
