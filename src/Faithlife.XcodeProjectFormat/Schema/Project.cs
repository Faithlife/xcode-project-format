using System.Text;
using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// An Xcode project, as stored in a <c>project.xcproj</c> file.
/// </summary>
public sealed class Project : IXCJsonEncodable, IXCJsonDecodable<Project>
{
	public Project(ConfigurationName defaultConfigurationName, ProjectLocalizationInfo localizationInfo)
	{
		DefaultConfigurationName = defaultConfigurationName ?? throw new ArgumentNullException(nameof(defaultConfigurationName));
		LocalizationInfo = localizationInfo ?? throw new ArgumentNullException(nameof(localizationInfo));
	}

	/// <summary>
	/// Reads a project from its JSON representation.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The JSON is invalid.</exception>
	public static Project FromJson(string json) => XCJsonDecoder.DecodeText<Project>(json ?? throw new ArgumentNullException(nameof(json)));

	/// <summary>
	/// Reads a project from its UTF-8 encoded JSON representation.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The JSON is invalid.</exception>
	public static Project FromJson(ReadOnlySpan<byte> utf8Json)
	{
		string json;
		try
		{
			json = s_utf8.GetString(utf8Json);
		}
		catch (DecoderFallbackException exception)
		{
			throw new XcodeProjectFormatException("The data is not valid UTF-8.", exception);
		}
		return FromJson(json);
	}

	/// <summary>
	/// Returns the JSON representation of the project.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The project is invalid.</exception>
	public string ToJson() => XCJsonEncoder.EncodeToText(this);

	/// <summary>
	/// Returns the UTF-8 encoded JSON representation of the project.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The project is invalid.</exception>
	public byte[] ToJsonUtf8() => XCJsonEncoder.EncodeToUtf8(this);

	public ObjectId? ObjectId { get; set; }

	public ObjectId? RootGroupDebugId { get; set; }

	public ObjectId? ConfigurationListDebugId { get; set; }

	/// <summary>
	/// The top-level references in the project's group tree.
	/// </summary>
	public List<Reference> TopLevelReferences { get; set; } = [];

	public List<SwiftPackage> Packages { get; set; } = [];

	public List<BuildConfiguration> Configurations { get; set; } = [];

	public ConfigurationName DefaultConfigurationName { get; set; }

	public Dictionary<string, BuildSetting> BuildSettings { get; set; } = [];

	public List<Target> Targets { get; set; } = [];

	public ProjectLocalizationInfo LocalizationInfo { get; set; }

	/// <summary>
	/// Capabilities that the version of Xcode must support to open the project.
	/// </summary>
	public HashSet<Capability> RequiredCapabilities { get; set; } = [];

	public bool BuildIndependentTargetsInParallel { get; set; } = true;

	public MarketingVersion? LastUpgradeCheck { get; set; }

	public MarketingVersion? LastSwiftUpdateCheck { get; set; }

	public MarketingVersion? LastSwiftMigration { get; set; }

	public string? OrganizationName { get; set; }

	public string? ClassPrefix { get; set; }

	/// <summary>
	/// The group that contains the products of the project's targets; defaults to the top-level <c>Products</c> group.
	/// </summary>
	public GroupTreeReference? ProductsGroup { get; set; } = DefaultProductsGroup;

	public List<RemoteProduct> ImportedProducts { get; set; } = [];

	/// <summary>
	/// The default value of <see cref="ProductsGroup"/>.
	/// </summary>
	public static GroupTreeReference DefaultProductsGroup => new(NamePath.FromChildNames(["Products"]));

	private void VerifyReferenceIntegrity()
	{
		var duplicateNames = Targets.GroupBy(x => x.Name, StringComparer.Ordinal).Where(x => x.Count() > 1).Select(x => x.Key).Order(XCJsonCoding.StringComparer).ToList();
		if (duplicateNames.Count != 0)
		{
			var isAre = duplicateNames.Count == 1 ? "is" : "are";
			throw new XcodeProjectFormatException($"Target names must be unique, but {JoinWithFinalSeparator(duplicateNames.Select(x => x.SmartQuoted()))} {isAre} used multiple times");
		}
	}

	internal static string JoinWithFinalSeparator(IEnumerable<string> values)
	{
		var list = values.ToList();
		return list.Count <= 1 ? string.Concat(list) : string.Join(", ", list.Take(list.Count - 1)) + " and " + list[^1];
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		VerifyReferenceIntegrity();
		var container = encoder.OpenKeyedContainer();
		container.EncodeIfNotEmpty("required-capabilities", RequiredCapabilities);
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeIfNotNull("root-group-debug-id", RootGroupDebugId);
		container.EncodeIfNotNull("configuration-list-debug-id", ConfigurationListDebugId);
		container.EncodeIfNotNull("organization", OrganizationName);
		container.EncodeIfNotNull("class-prefix", ClassPrefix);
		container.EncodeIfNotDefault("build-independent-targets-in-parallel", BuildIndependentTargetsInParallel, true);
		container.Encode("default-configuration", DefaultConfigurationName);
		if (Configurations.Count != 0)
			container.Encode("configurations", Configurations.WithCompactValueEncoding());
		container.Encode("localizations", LocalizationInfo);
		container.EncodeIfNotEmpty("imported-products", ImportedProducts);
		container.EncodeIfNotEmpty("packages", Packages);
		container.Encode("files", TopLevelReferences);
		container.EncodeIfNotEmpty("targets", Targets);
		container.EncodeIfNotEmpty("build-settings", BuildSettings);
		if (!Equals(ProductsGroup, DefaultProductsGroup))
			container.Encode("products-group", ProductsGroup);
		container.EncodeIfNotNull("last-upgrade", LastUpgradeCheck);
		container.EncodeIfNotNull("last-swift-update", LastSwiftUpdateCheck);
		container.EncodeIfNotNull("last-swift-migration", LastSwiftMigration);
	}

	static Project IXCJsonDecodable<Project>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		var requiredCapabilities = container.Decode("required-capabilities", new HashSet<Capability>());
		var unsatisfiedCapabilities = requiredCapabilities.Where(x => !x.IsSatisfied).Order().Select(x => x.CapabilityDescription).ToList();
		if (unsatisfiedCapabilities.Count != 0)
			throw new XcodeProjectFormatException("The project requires a newer version of Xcode with support for " + JoinWithFinalSeparator(unsatisfiedCapabilities));

		var objectId = container.DecodeOptional<ObjectId>("id");
		var rootGroupDebugId = container.DecodeOptional<ObjectId>("root-group-debug-id");
		var configurationListDebugId = container.DecodeOptional<ObjectId>("configuration-list-debug-id");
		var topLevelReferences = container.Decode<List<Reference>>("files");
		var packages = container.Decode("packages", new List<SwiftPackage>());
		var configurations = container.Decode("configurations", new List<BuildConfiguration>());
		var defaultConfigurationName = container.Decode<ConfigurationName>("default-configuration");
		var buildSettings = container.Decode("build-settings", new Dictionary<string, BuildSetting>());
		var targets = container.Decode("targets", new List<Target>());
		var project = new Project(defaultConfigurationName, container.Decode<ProjectLocalizationInfo>("localizations"))
		{
			RequiredCapabilities = requiredCapabilities,
			ObjectId = objectId,
			RootGroupDebugId = rootGroupDebugId,
			ConfigurationListDebugId = configurationListDebugId,
			TopLevelReferences = topLevelReferences,
			Packages = packages,
			Configurations = configurations,
			BuildSettings = buildSettings,
			Targets = targets,
			BuildIndependentTargetsInParallel = container.Decode("build-independent-targets-in-parallel", true),
			LastUpgradeCheck = container.DecodeOptionalValue<MarketingVersion>("last-upgrade"),
			LastSwiftUpdateCheck = container.DecodeOptionalValue<MarketingVersion>("last-swift-update"),
			LastSwiftMigration = container.DecodeOptionalValue<MarketingVersion>("last-swift-migration"),
			OrganizationName = container.DecodeOptional<string>("organization"),
			ClassPrefix = container.DecodeOptional<string>("class-prefix"),
			ProductsGroup = container.DecodeOptionalWithNonNullDefault("products-group", DefaultProductsGroup),
			ImportedProducts = container.Decode("imported-products", new List<RemoteProduct>()),
		};
		project.VerifyReferenceIntegrity();
		return project;
	}

	private static readonly UTF8Encoding s_utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
}
