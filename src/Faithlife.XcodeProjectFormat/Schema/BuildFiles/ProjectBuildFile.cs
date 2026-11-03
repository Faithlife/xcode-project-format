using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The membership of a file reference in a build phase of a target.
/// </summary>
public sealed class ProjectBuildFile : IXCJsonEncodable, IXCJsonDecodable<ProjectBuildFile>
{
	public ProjectBuildFile(ProjectBuildPhaseReference buildPhase) => BuildPhase = buildPhase ?? throw new ArgumentNullException(nameof(buildPhase));

	public ObjectId? ObjectId { get; set; }

	public ProjectBuildPhaseReference BuildPhase { get; set; }

	public BuildFileProperties Properties { get; set; } = new();

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (ObjectId is null && Properties.IsDefault && BuildPhase.GroupTreeReferenceRepresentation.EncodesToString)
		{
			((IXCJsonEncodable) BuildPhase).Encode(encoder);
		}
		else
		{
			var container = encoder.OpenKeyedContainer(XCJsonPrintingDensity.Compact);
			container.EncodeIfNotNull("id", ObjectId);
			container.Encode("build-phase", BuildPhase);
			container.EncodeInline(Properties);
		}
	}

	static ProjectBuildFile IXCJsonDecodable<ProjectBuildFile>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.String)
			return new ProjectBuildFile(decoder.Decode<ProjectBuildPhaseReference>());

		var container = decoder.OpenKeyedContainer();
		return new ProjectBuildFile(container.Decode<ProjectBuildPhaseReference>("build-phase"))
		{
			ObjectId = container.DecodeOptional<ObjectId>("id"),
			Properties = container.DecodeInline<BuildFileProperties>(),
		};
	}
}
