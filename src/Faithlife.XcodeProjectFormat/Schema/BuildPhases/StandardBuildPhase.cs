using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A build phase whose only properties are an identifier and a name.
/// </summary>
public abstract class StandardBuildPhase : BuildPhase
{
	private protected StandardBuildPhase()
	{
	}

	private protected override bool EncodesAsKindOnly => ObjectId is null && Name is null;

	private protected override XCJsonPrintingDensity? PrintingDensity => XCJsonPrintingDensity.Compact;
}
