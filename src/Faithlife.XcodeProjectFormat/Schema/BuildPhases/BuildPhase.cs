using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A build phase of a target.
/// </summary>
public abstract class BuildPhase : IXCJsonEncodable, IXCJsonDecodable<BuildPhase>
{
	/// <summary>
	/// Creates a build phase of the specified kind, with default properties.
	/// </summary>
	public static BuildPhase Create(BuildPhaseKind kind) => kind switch
	{
		BuildPhaseKind.AppleScript => new AppleScriptBuildPhase(),
		BuildPhaseKind.Frameworks => new FrameworksBuildPhase(),
		BuildPhaseKind.Headers => new HeadersBuildPhase(),
		BuildPhaseKind.JavaArchive => new JavaArchiveBuildPhase(),
		BuildPhaseKind.Resources => new ResourcesBuildPhase(),
		BuildPhaseKind.Rez => new RezBuildPhase(),
		BuildPhaseKind.Sources => new SourcesBuildPhase(),
		BuildPhaseKind.Copy => new CopyFilesBuildPhase(),
		BuildPhaseKind.Script => new ScriptBuildPhase(),
		_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
	};

	public abstract BuildPhaseKind Kind { get; }

	public ObjectId? ObjectId { get; set; }

	public string? Name { get; set; }

	private protected BuildPhase()
	{
	}

	private protected virtual bool EncodesAsKindOnly => false;

	private protected virtual XCJsonPrintingDensity? PrintingDensity => null;

	private protected virtual void EncodeProperties(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeIfNotNull("name", Name);
	}

	private protected virtual void DecodeProperties(XCJsonDecoder.KeyedContainer container)
	{
		ObjectId = container.DecodeOptional<ObjectId>("id");
		Name = container.DecodeOptional<string>("name");
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (EncodesAsKindOnly)
		{
			encoder.OpenPrimitiveContainer().Encode(XCJsonCoding.GetEnumName(Kind));
		}
		else
		{
			var container = encoder.OpenKeyedContainer(PrintingDensity);
			container.Encode("kind", Kind);
			EncodeProperties(container);
		}
	}

	static BuildPhase IXCJsonDecodable<BuildPhase>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.String)
		{
			var kind = XCJsonCoding.ParseEnumName<BuildPhaseKind>(decoder.DecodePrimitiveString());
			if (kind is BuildPhaseKind.AppleScript or BuildPhaseKind.Copy or BuildPhaseKind.Script)
				throw new XcodeProjectFormatException($"Invalid build phase encoding {XCJsonCoding.GetEnumName(kind).SmartQuoted()}. The accompanying build phase properties are missing.");
			return Create(kind);
		}

		var container = decoder.OpenKeyedContainer();
		var buildPhase = Create(container.Decode<BuildPhaseKind>("kind"));
		buildPhase.DecodeProperties(container);
		return buildPhase;
	}
}
