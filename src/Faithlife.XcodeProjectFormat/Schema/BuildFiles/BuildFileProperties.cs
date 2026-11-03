using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// Properties of a build file.
/// </summary>
public sealed class BuildFileProperties : IXCJsonInlineKeyedEncodable, IXCJsonInlineKeyedDecodable<BuildFileProperties>
{
	public HashSet<PlatformFilter> PlatformFilters { get; set; } = [];

	public BuildFileAttributes Attributes { get; set; } = new();

	public string? AdditionalBuildFlags { get; set; }

	public HashSet<AssetTag> AssetTags { get; set; } = [];

	/// <summary>
	/// True if every property has its default value.
	/// </summary>
	public bool IsDefault => PlatformFilters.Count == 0 && Attributes.IsDefault && AdditionalBuildFlags is null && AssetTags.Count == 0;

	void IXCJsonInlineKeyedEncodable.Encode(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotEmpty("platforms", PlatformFilters, XCJsonPrintingDensity.Compact);
		container.EncodeInline(Attributes);
		container.EncodeIfNotNull("arguments", AdditionalBuildFlags);
		container.EncodeIfNotEmpty("asset-tags", AssetTags, XCJsonPrintingDensity.Compact);
	}

	static BuildFileProperties IXCJsonInlineKeyedDecodable<BuildFileProperties>.Decode(XCJsonDecoder.KeyedContainer container) => new()
	{
		PlatformFilters = container.Decode("platforms", new HashSet<PlatformFilter>()),
		Attributes = container.DecodeInline<BuildFileAttributes>(),
		AdditionalBuildFlags = container.DecodeOptional<string>("arguments"),
		AssetTags = container.Decode("asset-tags", new HashSet<AssetTag>()),
	};
}
