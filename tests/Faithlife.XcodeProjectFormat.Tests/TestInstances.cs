using Faithlife.XcodeProjectFormat.Schema;
using static Faithlife.XcodeProjectFormat.Tests.TestUtilities;

namespace Faithlife.XcodeProjectFormat.Tests;

// Mirrors TestInstances.swift; each property returns a new instance because the schema types are mutable.
internal static class TestInstances
{
	public static AssetTag PopulatedAssetTag => new("release-asset");
	public static AssetTag EmptyAssetTag => new("");

	public static ObjectId PopulatedObjectId => new("0123456789ABCDEF");
	public static ObjectId EmptyObjectId => new("FEDCBA9876543210");

	public static ConfigurationName PopulatedConfigurationName => new("Debug");
	public static ConfigurationName EmptyConfigurationName => new("");

	public static BuildConfiguration PopulatedConfiguration => new(PopulatedConfigurationName) { File = PopulatedGroupTreeAnchoredReference, ObjectId = PopulatedObjectId };
	public static BuildConfiguration EmptyConfiguration => new(EmptyConfigurationName);

	public static BuildFileAttributes PopulatedBuildFileAttributes => new()
	{
		HeaderRole = HeaderRole.Public,
		MachInterfaceGeneration = MachInterfaceGeneration.Both,
		IsWeak = true,
		CodeSignOnCopy = true,
		CodeGeneration = CodeGeneration.Skip,
		HeaderPreservation = HeaderPreservation.RemoveOnCopy,
		Decompress = true,
		CodeGenerationVisibility = CodeGenerationVisibility.Public,
	};
	public static BuildFileAttributes EmptyBuildFileAttributes => new();

	public static BuildFileProperties PopulatedBuildFileProperties => new()
	{
		PlatformFilters = [PopulatedPlatformFilter],
		AdditionalBuildFlags = "--go-fast",
		AssetTags = [PopulatedAssetTag],
		Attributes = PopulatedBuildFileAttributes,
	};
	public static BuildFileProperties EmptyBuildFileProperties => new();

	public static ProjectBuildFile PopulatedProjectBuildFile => new(PopulatedProjectBuildPhaseReference) { ObjectId = PopulatedObjectId, Properties = PopulatedBuildFileProperties };
	public static ProjectBuildFile EmptyProjectBuildFile => new(EmptyProjectBuildPhaseReference);

	public static TargetBuildFile PopulatedTargetBuildFile => new(PopulatedTargetBuildPhaseReference) { ObjectId = PopulatedObjectId, Properties = PopulatedBuildFileProperties };
	public static TargetBuildFile EmptyTargetBuildFile => new(EmptyTargetBuildPhaseReference);

	public static T PopulatedStandardBuildPhase<T>()
		where T : StandardBuildPhase, new() => new() { ObjectId = PopulatedObjectId, Name = "Compile Sources" };

	public static AppleScriptBuildPhase PopulatedAppleScriptBuildPhase => new() { ObjectId = PopulatedObjectId, Name = "Run AppleScript", IsSharedContext = true, ContextName = "MyContext" };
	public static AppleScriptBuildPhase EmptyAppleScriptBuildPhase => new();

	public static CopyFilesBuildPhase PopulatedCopyFilesBuildPhase => new() { ObjectId = PopulatedObjectId, Name = "Copy Files", BundleBasePath = BundleBasePath.ResourcesDirectory, RelativePath = "Subfolder", Scope = BuildPhaseScope.Install };
	public static CopyFilesBuildPhase EmptyCopyFilesBuildPhase => new();

	public static ScriptBuildPhase PopulatedScriptBuildPhase => new()
	{
		ObjectId = PopulatedObjectId,
		Name = "Run Script",
		ShellPath = "/bin/sh",
		Script = "echo hello",
		LogEnvironmentVariables = true,
		InputPaths = ["$(SRCROOT)/in.txt"],
		InputFileListPaths = ["$(SRCROOT)/in.xcfilelist"],
		OutputPaths = ["$(DERIVED_FILE_DIR)/out.txt"],
		OutputFileListPaths = ["$(DERIVED_FILE_DIR)/out.xcfilelist"],
		DependencyFile = "$(DERIVED_FILE_DIR)/deps.d",
		RunOnEveryBuild = true,
		Scope = BuildPhaseScope.Install,
	};
	public static ScriptBuildPhase EmptyScriptBuildPhase => new() { Name = "", ShellPath = "", Script = "" };
	public static IEnumerable<ScriptBuildPhase> AdditionalScriptBuildPhases => s_scripts.Select(x =>
	{
		var phase = PopulatedScriptBuildPhase;
		phase.Script = x;
		return phase;
	});

	public static BuildPhase PopulatedBuildPhase => PopulatedScriptBuildPhase;
	public static BuildPhase EmptyBuildPhase => new SourcesBuildPhase();
	public static IEnumerable<BuildPhase> AdditionalBuildPhases =>
	[
		PopulatedStandardBuildPhase<FrameworksBuildPhase>(),
		PopulatedStandardBuildPhase<HeadersBuildPhase>(),
		PopulatedStandardBuildPhase<JavaArchiveBuildPhase>(),
		PopulatedStandardBuildPhase<ResourcesBuildPhase>(),
		PopulatedStandardBuildPhase<RezBuildPhase>(),
		PopulatedStandardBuildPhase<SourcesBuildPhase>(),
		PopulatedAppleScriptBuildPhase,
		PopulatedCopyFilesBuildPhase,
		PopulatedScriptBuildPhase,
		new FrameworksBuildPhase(),
		new HeadersBuildPhase(),
		new JavaArchiveBuildPhase(),
		new ResourcesBuildPhase(),
		new RezBuildPhase(),
		new SourcesBuildPhase(),
		EmptyAppleScriptBuildPhase,
		EmptyCopyFilesBuildPhase,
		EmptyScriptBuildPhase,
	];

	public static RemoteProduct PopulatedRemoteProduct => new(PopulatedGroupTreeReference, "MyFramework", PopulatedObjectId, "MyFramework.framework") { FileType = PopulatedFileTypeId, BuildFiles = [PopulatedProjectBuildFile] };
	public static RemoteProduct EmptyRemoteProduct => new(EmptyGroupTreeReference, "", EmptyObjectId, "") { FileType = EmptyFileTypeId };

	public static BuildRule PopulatedBuildRule => new("sh")
	{
		ObjectId = PopulatedObjectId,
		Name = "Custom Rule",
		FileType = PopulatedFileTypeId,
		FilePatterns = "*.swift",
		Script = "swiftc $INPUT_FILE_PATH",
		InputFiles = ["$(SRCROOT)/in.swift"],
		InputFileLists = ["$(SRCROOT)/in.xcfilelist"],
		OutputFiles = ["$(DERIVED_FILE_DIR)/out.o"],
		OutputFileLists = ["$(DERIVED_FILE_DIR)/out.xcfilelist"],
		OutputFilesCompilerFlags = ["-O"],
		DependencyFile = "$(DERIVED_FILE_DIR)/deps.d",
		RunOncePerArchitecture = true,
	};
	public static BuildRule EmptyBuildRule => new("") { RunOncePerArchitecture = false };
	public static IEnumerable<BuildRule> AdditionalBuildRules => s_scripts.Select(x =>
	{
		var rule = PopulatedBuildRule;
		rule.Script = x;
		return rule;
	});

	public static Capability PopulatedCapability => Capability.KnownCapabilityForTesting;
	public static Capability EmptyCapability => new("");

	public static Language PopulatedLanguage => new("en");
	public static Language EmptyLanguage => new("");

	public static FileTypeId PopulatedFileTypeId => new("sourcecode.swift");
	public static FileTypeId EmptyFileTypeId => new("");

	public static GroupTreeReference PopulatedGroupTreeReference => new(PopulatedNamePath);
	public static GroupTreeReference EmptyGroupTreeReference => new(EmptyNamePath);
	public static IEnumerable<GroupTreeReference> AdditionalGroupTreeReferences => AdditionalNamePaths.Select(x => new GroupTreeReference(x)).Prepend(new GroupTreeReference(PopulatedObjectId));

	public static GroupTreeAnchoredReference PopulatedGroupTreeAnchoredReference => new(PopulatedGroupTreeReference, PopulatedNamePath);
	public static GroupTreeAnchoredReference EmptyGroupTreeAnchoredReference => new(EmptyGroupTreeReference);
	public static IEnumerable<GroupTreeAnchoredReference> AdditionalGroupTreeAnchoredReferences => [new(EmptyGroupTreeReference, EmptyNamePath)];

	public static LocalTargetReference PopulatedLocalTargetReference => new("App");
	public static LocalTargetReference EmptyLocalTargetReference => new("");

	public static NamePath PopulatedNamePath => new(NamePathComponent.Parent, NamePathComponent.Child("Sources"), NamePathComponent.Child("Foo.swift"));
	public static NamePath EmptyNamePath => new();
	public static IEnumerable<NamePath> AdditionalNamePaths => EnumerateSubsetPermutations(
		[
			NamePathComponent.Current,
			NamePathComponent.Parent,
			NamePathComponent.Child("item"),
			NamePathComponent.Child(".."),
			NamePathComponent.Child("."),
			NamePathComponent.Child("/"),
			NamePathComponent.Child(""),
		]).Select(x => new NamePath(x));

	public static NamePathComponent PopulatedNamePathComponent => NamePathComponent.Parent;
	public static NamePathComponent EmptyNamePathComponent => NamePathComponent.Child("");
	public static IEnumerable<NamePathComponent> AdditionalNamePathComponents => [NamePathComponent.Child(""), NamePathComponent.Child("Asdf"), NamePathComponent.Current, NamePathComponent.Parent];

	public static PlatformFilter PopulatedPlatformFilter => new("ios");
	public static PlatformFilter EmptyPlatformFilter => new("");

	public static ProductTypeId PopulatedProductTypeId => new("com.apple.product-type.application");
	public static ProductTypeId EmptyProductTypeId => new("");

	public static BuildSetting PopulatedBuildSetting => new("Setting");
	public static BuildSetting EmptyBuildSetting => new(Array.Empty<string>());
	public static IEnumerable<BuildSetting> AdditionalBuildSettings => [new(""), new(["String"]), new(["A", "B"])];

	public static Project PopulatedProject => new(PopulatedConfigurationName, PopulatedProjectLocalizationInfo)
	{
		ObjectId = PopulatedObjectId,
		RootGroupDebugId = PopulatedObjectId,
		ConfigurationListDebugId = PopulatedObjectId,
		TopLevelReferences = [PopulatedReference],
		Packages = [PopulatedSwiftPackage],
		Configurations = [PopulatedConfiguration, EmptyConfiguration],
		BuildSettings = new() { ["SDKROOT"] = PopulatedBuildSetting },
		Targets = [PopulatedTarget],
		RequiredCapabilities = [PopulatedCapability],
		BuildIndependentTargetsInParallel = true,
		LastUpgradeCheck = PopulatedMarketingVersion,
		LastSwiftUpdateCheck = PopulatedMarketingVersion,
		LastSwiftMigration = PopulatedMarketingVersion,
		OrganizationName = "example.com",
		ClassPrefix = "EX",
		ProductsGroup = PopulatedGroupTreeReference,
		ImportedProducts = [PopulatedRemoteProduct],
	};
	public static Project EmptyProject => new(EmptyConfigurationName, EmptyProjectLocalizationInfo) { BuildIndependentTargetsInParallel = false, ProductsGroup = null };

	public static ProjectLocalizationInfo PopulatedProjectLocalizationInfo => new(new Language("en"), [new Language("Base")]);
	public static ProjectLocalizationInfo EmptyProjectLocalizationInfo => new(new Language("en"));

	public static RemoteTargetDependency PopulatedRemoteTarget => new(PopulatedGroupTreeReference, "OtherTarget", PopulatedObjectId);
	public static RemoteTargetDependency EmptyRemoteTarget => new(EmptyGroupTreeReference, "", EmptyObjectId);

	public static SwiftPackageName PopulatedSwiftPackageName => new("SuperUseful");
	public static SwiftPackageName EmptySwiftPackageName => new("");

	public static SwiftPackageProductReference PopulatedSwiftPackageProductReference => new("helper", PopulatedSwiftPackageName, SwiftPackageProductType.BuildToolPlugin, PopulatedObjectId);
	public static SwiftPackageProductReference EmptySwiftPackageProductReference => new("", EmptySwiftPackageName, SwiftPackageProductType.Other);

	public static LocalSwiftPackage PopulatedLocalSwiftPackage => new("./LocalPackage");
	public static LocalSwiftPackage EmptyLocalSwiftPackage => new("");

	public static RemoteSwiftPackage PopulatedRemoteSwiftPackage => new("https://github.com/example/Package.git") { VersionConstraint = PopulatedSwiftPackageVersionConstraint };
	public static RemoteSwiftPackage EmptyRemoteSwiftPackage => new("");

	public static SwiftPackageVersionConstraint PopulatedSwiftPackageVersionConstraint => SwiftPackageVersionConstraint.UpToNextMajorVersion("1.4");
	public static SwiftPackageVersionConstraint EmptySwiftPackageVersionConstraint => SwiftPackageVersionConstraint.Branch("");
	public static IEnumerable<SwiftPackageVersionConstraint> AdditionalSwiftPackageVersionConstraints =>
	[
		SwiftPackageVersionConstraint.Revision("1"),
		SwiftPackageVersionConstraint.Branch("2"),
		SwiftPackageVersionConstraint.Version("3"),
		SwiftPackageVersionConstraint.VersionRange("4", "5"),
		SwiftPackageVersionConstraint.UpToNextMinorVersion("6"),
		SwiftPackageVersionConstraint.UpToNextMajorVersion("7"),
	];

	public static SwiftPackage PopulatedSwiftPackage => new(PopulatedRemoteSwiftPackage);
	public static SwiftPackage EmptySwiftPackage => new(EmptyLocalSwiftPackage);

	public static T WithPopulatedTargetProperties<T>(T target)
		where T : Target
	{
		target.ConfigurationListDebugId = PopulatedObjectId;
		target.Dependencies = [PopulatedTargetDependency];
		target.BuildPhases = [PopulatedBuildPhase];
		target.BuildRules = [PopulatedBuildRule];
		target.SpecializedConfigurations = [PopulatedConfiguration, PopulatedConfiguration];
		target.BuildSettings = new() { ["SWIFT_VERSION"] = PopulatedBuildSetting };
		target.ProductType = PopulatedProductTypeId;
		target.TestHostTarget = PopulatedLocalTargetReference;
		target.LegacyProvisioningStyle = LegacyProvisioningStyle.Automatic;
		target.LegacyTeamId = "MyTeamID";
		target.LastSwiftUpdateCheck = PopulatedMarketingVersion;
		target.LastSwiftMigration = PopulatedMarketingVersion;
		target.PackageProductTargetMembers = [PopulatedSwiftPackageProductTargetMember];
		return target;
	}

	public static T WithEmptyTargetProperties<T>(T target)
		where T : Target
	{
		target.Dependencies = [EmptyTargetDependency];
		target.BuildPhases = [EmptyBuildPhase];
		target.BuildRules = [EmptyBuildRule];
		return target;
	}

	public static Target PopulatedNativeTarget => WithPopulatedTargetProperties(new Target("App", PopulatedObjectId));
	public static Target EmptyNativeTarget => WithEmptyTargetProperties(new Target("", EmptyObjectId));
	public static IEnumerable<Target> AdditionalNativeTargets =>
		new ProductTypeId?[] { new("unusual.prefix"), ProductTypeId.FromAbbreviatedRepresentation("app"), null }.Select(x =>
		{
			var target = EmptyNativeTarget;
			target.ProductType = x;
			return target;
		});

	public static Target PopulatedTarget => PopulatedNativeTarget;
	public static Target EmptyTarget => WithPopulatedTargetProperties(new Target("App", PopulatedObjectId, TargetKind.Aggregate));
	public static IEnumerable<Target> AdditionalTargets => [PopulatedExternalBuildSystemTarget];

	public static ExternalBuildSystemTarget PopulatedExternalBuildSystemTarget => WithPopulatedTargetProperties(new ExternalBuildSystemTarget("App", PopulatedObjectId, "/usr/bin/external-tool")
	{
		BuildToolArguments = "--glow-in-the-dark true",
		BuildToolWorkingDirectory = "/shared/build",
		PassBuildSettingsInEnvironment = true,
	});
	public static ExternalBuildSystemTarget EmptyExternalBuildSystemTarget => WithEmptyTargetProperties(new ExternalBuildSystemTarget("", EmptyObjectId, "") { PassBuildSettingsInEnvironment = false });
	public static IEnumerable<ExternalBuildSystemTarget> AdditionalExternalBuildSystemTargets
	{
		get
		{
			var target = EmptyExternalBuildSystemTarget;
			target.BuildToolWorkingDirectory = "";
			return [target];
		}
	}

	public static SwiftPackageProductTargetMember PopulatedSwiftPackageProductTargetMember => new(PopulatedSwiftPackageProductReference, PopulatedTargetBuildFile);
	public static SwiftPackageProductTargetMember EmptySwiftPackageProductTargetMember => new(EmptySwiftPackageProductReference, EmptyTargetBuildFile);

	public static TargetDependency PopulatedTargetDependency => WithPlatforms(PopulatedRemoteTarget, [PopulatedPlatformFilter]);
	public static TargetDependency EmptyTargetDependency => new LocalTargetDependency(EmptyLocalTargetReference);
	public static IEnumerable<TargetDependency> AdditionalTargetDependencies =>
	[
		WithPlatforms(new LocalTargetDependency(PopulatedLocalTargetReference), [PopulatedPlatformFilter]),
		WithPlatforms(PopulatedRemoteTarget, [PopulatedPlatformFilter]),
		WithPlatforms(new PackageProductDependency(PopulatedSwiftPackageProductReference), [PopulatedPlatformFilter]),
		new LocalTargetDependency(PopulatedLocalTargetReference),
		PopulatedRemoteTarget,
		new PackageProductDependency(PopulatedSwiftPackageProductReference),
	];

	public static FileReference PopulatedFileReference => new(PopulatedFilePath)
	{
		ObjectId = PopulatedObjectId,
		ExplicitFileType = PopulatedFileTypeId,
		ExpectedSignature = "abc123",
		TextEncoding = PopulatedTextEncoding,
		LineEnding = LineEnding.LineFeed,
		IncludeInIndex = true,
		BuildFiles = [PopulatedProjectBuildFile],
	};
	public static FileReference EmptyFileReference => new(EmptyFilePath);

	public static TextEncoding PopulatedTextEncoding => TextEncoding.Utf8;
	public static TextEncoding EmptyTextEncoding => TextEncoding.Ascii;

	public static ProjectBuildPhaseReference PopulatedProjectBuildPhaseReference => new(PopulatedLocalTargetReference, BuildPhaseKind.Script, "Install man pages");
	public static ProjectBuildPhaseReference EmptyProjectBuildPhaseReference => new(EmptyLocalTargetReference, BuildPhaseKind.Sources);

	public static TargetBuildPhaseReference PopulatedTargetBuildPhaseReference => new(BuildPhaseKind.Copy, "Install man pages");
	public static TargetBuildPhaseReference EmptyTargetBuildPhaseReference => new(BuildPhaseKind.Headers);

	public static T WithPopulatedExceptionSetProperties<T>(T exceptionSet)
		where T : FolderExceptionSet
	{
		exceptionSet.Sense = ExceptionSetSense.Exclusions;
		exceptionSet.MembershipExceptions = [new FolderMemberId("File1.swift")];
		exceptionSet.PlatformFiltersByFolderMemberId = new() { [new FolderMemberId("File1.swift")] = [PopulatedPlatformFilter] };
		exceptionSet.AttributesByFolderMemberId = new() { [new FolderMemberId("File1.swift")] = PopulatedBuildFileAttributes };
		exceptionSet.AssetTagsByFolderMemberId = new() { [new FolderMemberId("File1.swift")] = [PopulatedAssetTag] };
		return exceptionSet;
	}

	public static T WithEmptyExceptionSetProperties<T>(T exceptionSet)
		where T : FolderExceptionSet
	{
		exceptionSet.Sense = ExceptionSetSense.Inclusions;
		return exceptionSet;
	}

	public static BuildPhaseExceptionSet PopulatedBuildPhaseExceptionSet => WithPopulatedExceptionSetProperties(new BuildPhaseExceptionSet(PopulatedProjectBuildPhaseReference));
	public static BuildPhaseExceptionSet EmptyBuildPhaseExceptionSet => WithEmptyExceptionSetProperties(new BuildPhaseExceptionSet(EmptyProjectBuildPhaseReference));

	public static FolderExceptionSet PopulatedFolderExceptionSet => PopulatedBuildPhaseExceptionSet;
	public static FolderExceptionSet EmptyFolderExceptionSet => EmptyTargetExceptionSet;

	public static TargetExceptionSet PopulatedTargetExceptionSet => WithPopulatedExceptionSetProperties(new TargetExceptionSet(PopulatedLocalTargetReference)
	{
		PublicHeaders = [new FolderMemberId("Foo.h")],
		PrivateHeaders = [new FolderMemberId("Bar.h")],
		AdditionalCompilerFlags = new() { [new FolderMemberId("Foo.h")] = "-O" },
	});
	public static TargetExceptionSet EmptyTargetExceptionSet => WithEmptyExceptionSetProperties(new TargetExceptionSet(EmptyLocalTargetReference));

	public static Folder PopulatedFolder => new(PopulatedFilePath)
	{
		ObjectId = PopulatedObjectId,
		Targets = [PopulatedLocalTargetReference],
		MembershipExceptions = [PopulatedTargetExceptionSet],
		ExplicitFileTypes = new() { [new FolderMemberId("File1.swift")] = PopulatedFileTypeId },
		ExplicitOpaqueFolders = [new FolderMemberId("OpaqueFolder")],
		IncludeInIndex = true,
	};
	public static Folder EmptyFolder => new(EmptyFilePath) { ObjectId = PopulatedObjectId };

	public static FolderMemberId PopulatedFolderMemberId => new("Sources/Foo.swift");
	public static FolderMemberId EmptyFolderMemberId => new("");

	public static Group PopulatedGroup => new(PopulatedFilePath, "Sources") { ObjectId = PopulatedObjectId, IncludeInIndex = true, Children = [PopulatedFileReference] };
	public static Group EmptyGroup => new(EmptyFilePath, "");

	public static VariantGroup PopulatedVariantGroup => new("Variants", PopulatedFilePath) { ObjectId = PopulatedObjectId, IncludeInIndex = true, BuildFiles = [PopulatedProjectBuildFile], Children = [PopulatedFileReference] };
	public static VariantGroup EmptyVariantGroup => new("", EmptyFilePath);

	public static VersionGroup PopulatedVersionGroup => new(PopulatedFilePath, "Model")
	{
		ObjectId = PopulatedObjectId,
		CurrentVersion = PopulatedGroupTreeReference,
		VersionedFileType = PopulatedFileTypeId,
		IncludeInIndex = true,
		BuildFiles = [PopulatedProjectBuildFile],
		Children = [PopulatedFileReference],
	};
	public static VersionGroup EmptyVersionGroup => new(EmptyFilePath, "");

	public static FilePath PopulatedFilePath => new(FilePathBase.Absolute, "/Sources/Foo.swift");
	public static FilePath EmptyFilePath => new(FilePathBase.Group, "");
	public static IEnumerable<FilePath> AdditionalFilePaths
	{
		get
		{
			FilePathBase[] relativeBases = [FilePathBase.Group, FilePathBase.Project, FilePathBase.Developer, FilePathBase.BuildProducts, FilePathBase.Sdk];
			string[] fragments = ["/", "A", "B", "<", ">", "USER", "\\"];
			foreach (var path in AllSubsetPermutationJoinings(fragments))
			{
				if (!path.StartsWith('/'))
				{
					foreach (var relativeBase in relativeBases)
						yield return new FilePath(relativeBase, path);
					yield return new FilePath(FilePathBase.SourceRoot(path), path);
				}
			}
			foreach (var path in AllSubsetPermutationJoinings(fragments))
				yield return new FilePath(FilePathBase.Absolute, "/" + path);
		}
	}

	public static Reference PopulatedReference => PopulatedFileReference;
	public static Reference EmptyReference => EmptyGroup;
	public static IEnumerable<Reference> AdditionalReferences => [PopulatedFileReference, PopulatedGroup, PopulatedFolder, PopulatedVariantGroup, PopulatedVersionGroup];

	public static MultilineText PopulatedMultilineText => new("#!/usr/bin/sh\necho \"Hello World\"\n");
	public static MultilineText EmptyMultilineText => new("");
	public static IEnumerable<MultilineText> AdditionalMultilineTexts => [new(""), new("\n"), new("\n\n"), new("\n\n\n")];

	public static MarketingVersion PopulatedMarketingVersion => new(27, 1, 2);
	public static MarketingVersion EmptyMarketingVersion => new(27, 0, 0);
	public static IEnumerable<MarketingVersion> AdditionalMarketingVersions => [new(0, 0, 0), new(1, 0, 0)];

	private static T WithPlatforms<T>(T dependency, HashSet<PlatformFilter> platformFilters)
		where T : TargetDependency
	{
		dependency.PlatformFilters = platformFilters;
		return dependency;
	}

	private static readonly string[] s_scripts = ["echo hello", "echo \"hello\"", "echo \"hello\"\n", "echo \"hello\"\n\n", "echo \"hello\"\nexit -1\n"];
}
