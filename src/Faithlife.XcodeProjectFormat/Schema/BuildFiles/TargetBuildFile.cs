using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The membership of a file in a build phase of a target, from the perspective of that target.
/// </summary>
public sealed class TargetBuildFile : IXCJsonEncodable, IXCJsonDecodable<TargetBuildFile>
{
	public TargetBuildFile(TargetBuildPhaseReference buildPhase) => BuildPhase = buildPhase ?? throw new ArgumentNullException(nameof(buildPhase));

	public ObjectId? ObjectId { get; set; }

	public TargetBuildPhaseReference BuildPhase { get; set; }

	public BuildFileProperties Properties { get; set; } = new();

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer(XCJsonPrintingDensity.Compact);
		container.EncodeIfNotNull("id", ObjectId);
		container.Encode("build-phase", BuildPhase);
		container.EncodeInline(Properties);
	}

	static TargetBuildFile IXCJsonDecodable<TargetBuildFile>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		return new TargetBuildFile(container.Decode<TargetBuildPhaseReference>("build-phase"))
		{
			ObjectId = container.DecodeOptional<ObjectId>("id"),
			Properties = container.DecodeInline<BuildFileProperties>(),
		};
	}
}
