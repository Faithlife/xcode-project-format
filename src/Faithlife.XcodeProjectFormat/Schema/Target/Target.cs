using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The kind of a target.
/// </summary>
public enum TargetKind
{
	[XCJsonName("native")]
	Native,

	[XCJsonName("aggregate")]
	Aggregate,

	[XCJsonName("external-build-system")]
	ExternalBuildSystem,
}

/// <summary>
/// A target of a project.
/// </summary>
public class Target : IXCJsonEncodable, IXCJsonDecodable<Target>
{
	/// <summary>
	/// Creates a native or aggregate target.
	/// </summary>
	public Target(string name, ObjectId objectId, TargetKind kind = TargetKind.Native)
	{
		if (kind == TargetKind.ExternalBuildSystem && GetType() != typeof(ExternalBuildSystemTarget))
			throw new ArgumentException("Use ExternalBuildSystemTarget to create an external build system target.", nameof(kind));
		Name = name ?? throw new ArgumentNullException(nameof(name));
		ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));
		Kind = kind;
	}

	public TargetKind Kind { get; }

	public string Name { get; set; }

	public ObjectId ObjectId { get; set; }

	public ObjectId? ConfigurationListDebugId { get; set; }

	public List<TargetDependency> Dependencies { get; set; } = [];

	public List<BuildPhase> BuildPhases { get; set; } = [];

	public List<BuildRule> BuildRules { get; set; } = [];

	/// <summary>
	/// Configurations that have a base configuration file or object identifier for this target.
	/// </summary>
	public List<BuildConfiguration> SpecializedConfigurations { get; set; } = [];

	public Dictionary<string, BuildSetting> BuildSettings { get; set; } = [];

	public GroupTreeReference? Product { get; set; }

	public ProductTypeId? ProductType { get; set; }

	public LocalTargetReference? TestHostTarget { get; set; }

	public LegacyProvisioningStyle? LegacyProvisioningStyle { get; set; }

	public string? LegacyTeamId { get; set; }

	public MarketingVersion? LastSwiftUpdateCheck { get; set; }

	public MarketingVersion? LastSwiftMigration { get; set; }

	public List<SwiftPackageProductTargetMember> PackageProductTargetMembers { get; set; } = [];

	private protected virtual void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		foreach (var configuration in SpecializedConfigurations)
		{
			if (!configuration.IsSpecialized)
				throw new XcodeProjectFormatException($"Redundant configuration specialization for {configuration.Name.Name.SmartQuoted()} in {Name.SmartQuoted()}");
		}

		container.Encode("name", Name);
		container.Encode("id", ObjectId);
		container.EncodeIfNotNull("configuration-list-debug-id", ConfigurationListDebugId);
		container.EncodeIfNotDefault("kind", Kind, TargetKind.Native);
		container.EncodeIfNotNull("product", Product);
		if (ProductType?.AbbreviatedRepresentation is { } abbreviatedProductType)
			container.Encode("product-type", abbreviatedProductType);
		else
			container.EncodeIfNotNull("full-product-type", ProductType);
		container.EncodeIfNotNull("last-swift-update", LastSwiftUpdateCheck);
		container.EncodeIfNotNull("last-swift-migration", LastSwiftMigration);
		container.EncodeIfNotNull("legacy-provisioning-style", LegacyProvisioningStyle);
		container.EncodeIfNotNull("legacy-team-id", LegacyTeamId);
		container.EncodeIfNotNull("test-host-target", TestHostTarget);
		if (SpecializedConfigurations.Count != 0)
			container.Encode("specialized-configurations", SpecializedConfigurations.WithCompactValueEncoding());
		container.EncodeIfNotEmpty("dependencies", Dependencies);
		container.EncodeIfNotEmpty("build-phases", BuildPhases);
		container.EncodeIfNotEmpty("build-rules", BuildRules);
		container.EncodeIfNotEmpty("package-product-members", PackageProductTargetMembers.Order(Comparer<SwiftPackageProductTargetMember>.Create(SwiftPackageProductTargetMember.CompareForEncoding)).ToList());
		container.EncodeIfNotEmpty("build-settings", BuildSettings);
	}

	private protected void DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		Product = container.DecodeOptional<GroupTreeReference>("product");
		ProductType = container.Contains("product-type") ? ProductTypeId.FromAbbreviatedRepresentation(container.Decode<string>("product-type")) : container.DecodeOptional<ProductTypeId>("full-product-type");
		TestHostTarget = container.DecodeOptional<LocalTargetReference>("test-host-target");
		LastSwiftUpdateCheck = container.DecodeOptionalValue<MarketingVersion>("last-swift-update");
		LastSwiftMigration = container.DecodeOptionalValue<MarketingVersion>("last-swift-migration");
		LegacyProvisioningStyle = container.DecodeOptionalValue<LegacyProvisioningStyle>("legacy-provisioning-style");
		LegacyTeamId = container.DecodeOptional<string>("legacy-team-id");
		Dependencies = container.Decode("dependencies", new List<TargetDependency>());
		BuildRules = container.Decode("build-rules", new List<BuildRule>());
		BuildPhases = container.Decode("build-phases", new List<BuildPhase>());
		SpecializedConfigurations = container.Decode("specialized-configurations", new List<BuildConfiguration>());
		BuildSettings = container.Decode("build-settings", new Dictionary<string, BuildSetting>());
		PackageProductTargetMembers = container.Decode("package-product-members", new List<SwiftPackageProductTargetMember>());
		ConfigurationListDebugId = container.DecodeOptional<ObjectId>("configuration-list-debug-id");
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => EncodeFields(encoder.OpenKeyedContainer());

	static Target IXCJsonDecodable<Target>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		var kind = container.Decode("kind", TargetKind.Native);
		var name = container.Decode<string>("name");
		var objectId = container.Decode<ObjectId>("id");
		if (kind == TargetKind.ExternalBuildSystem)
			return ExternalBuildSystemTarget.DecodeFields(container, name, objectId);

		var target = new Target(name, objectId, kind);
		target.DecodeFields(container);
		return target;
	}
}
