using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// An AppleScript build phase.
/// </summary>
public sealed class AppleScriptBuildPhase : BuildPhase
{
	public override BuildPhaseKind Kind => BuildPhaseKind.AppleScript;

	public bool IsSharedContext { get; set; }

	public string ContextName { get; set; } = "";

	private protected override void EncodeProperties(XCJsonEncoder.KeyedContainer container)
	{
		base.EncodeProperties(container);
		container.EncodeIfNotDefault("is-shared-context", IsSharedContext, false);
		container.EncodeIfNotDefault("context-name", ContextName, "");
	}

	private protected override void DecodeProperties(XCJsonDecoder.KeyedContainer container)
	{
		base.DecodeProperties(container);
		IsSharedContext = container.Decode("is-shared-context", false);
		ContextName = container.Decode("context-name", "");
	}
}
