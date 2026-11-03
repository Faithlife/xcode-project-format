using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A build configuration (for example, Debug or Release).
/// </summary>
public sealed class BuildConfiguration : IXCJsonEncodable, IXCJsonDecodable<BuildConfiguration>
{
	public BuildConfiguration(ConfigurationName name) => Name = name ?? throw new ArgumentNullException(nameof(name));

	public ConfigurationName Name { get; set; }

	/// <summary>
	/// The base configuration (<c>.xcconfig</c>) file.
	/// </summary>
	public GroupTreeAnchoredReference? File { get; set; }

	public ObjectId? ObjectId { get; set; }

	/// <summary>
	/// True if the configuration has a base configuration file or an object identifier.
	/// </summary>
	public bool IsSpecialized => File is not null || ObjectId is not null;

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (ObjectId is null && File is null)
		{
			((IXCJsonEncodable) Name).Encode(encoder);
		}
		else
		{
			var container = encoder.OpenKeyedContainer();
			container.EncodeIfNotNull("id", ObjectId);
			container.Encode("name", Name);
			container.EncodeIfNotNull("file", File, XCJsonPrintingDensity.Compact);
		}
	}

	static BuildConfiguration IXCJsonDecodable<BuildConfiguration>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.String)
			return new BuildConfiguration(decoder.Decode<ConfigurationName>());

		var container = decoder.OpenKeyedContainer();
		var objectId = container.DecodeOptional<ObjectId>("id");
		return new BuildConfiguration(container.Decode<ConfigurationName>("name"))
		{
			ObjectId = objectId,
			File = container.DecodeOptional<GroupTreeAnchoredReference>("file"),
		};
	}
}
