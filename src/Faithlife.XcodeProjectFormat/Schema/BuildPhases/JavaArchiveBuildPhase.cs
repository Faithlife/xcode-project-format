namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A "Java Archive" build phase.
/// </summary>
public sealed class JavaArchiveBuildPhase : StandardBuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.JavaArchive;
}
