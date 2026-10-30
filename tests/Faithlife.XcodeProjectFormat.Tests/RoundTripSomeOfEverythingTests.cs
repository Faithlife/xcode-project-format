using Faithlife.XcodeProjectFormat.Schema;
using static Faithlife.XcodeProjectFormat.Tests.TestInstances;
using static Faithlife.XcodeProjectFormat.Tests.TestUtilities;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class RoundTripSomeOfEverythingTests
{
	[Test]
	public void Enums()
	{
		TestEnum<BuildPhaseKind>();
		TestEnum<BuildPhaseScope>();
		TestEnum<BundleBasePath>();
		TestEnum<CodeGeneration>();
		TestEnum<CodeGenerationVisibility>();
		TestEnum<HeaderPreservation>();
		TestEnum<HeaderRole>();
		TestEnum<LegacyProvisioningStyle>();
		TestEnum<LineEnding>();
		TestEnum<MachInterfaceGeneration>();
		TestEnum<SwiftPackageProductType>();
		TestEnum<Reference.ReferenceKind>();
	}

	[Test]
	public void Identifiers()
	{
		Test(PopulatedAssetTag, EmptyAssetTag);
		Test(PopulatedCapability, EmptyCapability);
		Test(PopulatedConfigurationName, EmptyConfigurationName);
		Test(PopulatedFileTypeId, EmptyFileTypeId);
		Test(PopulatedFolderMemberId, EmptyFolderMemberId);
		Test(PopulatedLanguage, EmptyLanguage);
		Test(PopulatedLocalTargetReference, EmptyLocalTargetReference);
		Test(PopulatedObjectId, EmptyObjectId);
		Test(PopulatedPlatformFilter, EmptyPlatformFilter);
		Test(PopulatedProductTypeId, EmptyProductTypeId);
		Test(PopulatedSwiftPackageName, EmptySwiftPackageName);
	}

	[Test]
	public void Values()
	{
		Test(PopulatedBuildSetting, EmptyBuildSetting, AdditionalBuildSettings);
		Test(PopulatedMarketingVersion, EmptyMarketingVersion, AdditionalMarketingVersions);
		Test(PopulatedMultilineText, EmptyMultilineText, AdditionalMultilineTexts);
		Test(PopulatedNamePathComponent, EmptyNamePathComponent, AdditionalNamePathComponents);
		Test(PopulatedProjectLocalizationInfo, EmptyProjectLocalizationInfo);
		Test(PopulatedTextEncoding, EmptyTextEncoding);
	}

	[Test]
	public void Paths()
	{
		Test(PopulatedFilePath, EmptyFilePath, AdditionalFilePaths);
		Test(PopulatedGroupTreeAnchoredReference, EmptyGroupTreeAnchoredReference, AdditionalGroupTreeAnchoredReferences);
		Test(PopulatedGroupTreeReference, EmptyGroupTreeReference, AdditionalGroupTreeReferences);
		Test(PopulatedNamePath, EmptyNamePath, AdditionalNamePaths);
		Test(PopulatedProjectBuildPhaseReference, EmptyProjectBuildPhaseReference);
		Test(PopulatedTargetBuildPhaseReference, EmptyTargetBuildPhaseReference);
	}

	[Test]
	public void BuildFiles()
	{
		Test(PopulatedBuildFileAttributes, EmptyBuildFileAttributes);
		Test(PopulatedBuildFileProperties, EmptyBuildFileProperties);
		Test(PopulatedProjectBuildFile, EmptyProjectBuildFile);
		Test(PopulatedTargetBuildFile, EmptyTargetBuildFile);
	}

	[Test]
	public void BuildPhases()
	{
		Test(PopulatedBuildPhase, EmptyBuildPhase, AdditionalBuildPhases);
		Test(PopulatedAppleScriptBuildPhase, EmptyAppleScriptBuildPhase);
		Test(PopulatedCopyFilesBuildPhase, EmptyCopyFilesBuildPhase);
		Test(PopulatedScriptBuildPhase, EmptyScriptBuildPhase, AdditionalScriptBuildPhases);
		Test(PopulatedStandardBuildPhase<SourcesBuildPhase>(), new SourcesBuildPhase());
		Test(PopulatedBuildRule, EmptyBuildRule, AdditionalBuildRules);
	}

	[Test]
	public void References()
	{
		Test(PopulatedReference, EmptyReference, AdditionalReferences);
		Test(PopulatedFileReference, EmptyFileReference);
		Test(PopulatedFolder, EmptyFolder);
		Test(PopulatedGroup, EmptyGroup);
		Test(PopulatedVariantGroup, EmptyVariantGroup);
		Test(PopulatedVersionGroup, EmptyVersionGroup);
		Test(PopulatedFolderExceptionSet, EmptyFolderExceptionSet);
		Test(PopulatedBuildPhaseExceptionSet, EmptyBuildPhaseExceptionSet);
		Test(PopulatedTargetExceptionSet, EmptyTargetExceptionSet);
	}

	[Test]
	public void Packages()
	{
		Test(PopulatedLocalSwiftPackage, EmptyLocalSwiftPackage);
		Test(PopulatedRemoteSwiftPackage, EmptyRemoteSwiftPackage);
		Test<SwiftPackageLocation>(PopulatedRemoteSwiftPackage, EmptyLocalSwiftPackage);
		Test(PopulatedSwiftPackage, EmptySwiftPackage);
		Test(PopulatedSwiftPackageProductReference, EmptySwiftPackageProductReference);
		Test(PopulatedSwiftPackageProductTargetMember, EmptySwiftPackageProductTargetMember);
		Test(PopulatedSwiftPackageVersionConstraint, EmptySwiftPackageVersionConstraint, AdditionalSwiftPackageVersionConstraints);
	}

	[Test]
	public void Targets()
	{
		Test(PopulatedConfiguration, EmptyConfiguration);
		Test(PopulatedRemoteProduct, EmptyRemoteProduct);
		Test(PopulatedRemoteTarget, EmptyRemoteTarget);
		Test(PopulatedTargetDependency, EmptyTargetDependency, AdditionalTargetDependencies);
		Test(PopulatedNativeTarget, EmptyNativeTarget, AdditionalNativeTargets);
		Test(PopulatedTarget, EmptyTarget, AdditionalTargets);
		Test(PopulatedExternalBuildSystemTarget, EmptyExternalBuildSystemTarget, AdditionalExternalBuildSystemTargets);
	}

	[Test]
	public void Projects() => Test(PopulatedProject, EmptyProject);

	private static void Test<T>(T populated, T empty, IEnumerable<T>? additional = null)
	{
		AssertRoundTripEqual(populated);
		AssertRoundTripEqual(empty);
		foreach (var value in additional ?? [])
			AssertRoundTripEqual(value);
	}

	private static void TestEnum<T>()
		where T : struct, Enum
	{
		foreach (var value in Enum.GetValues<T>())
			AssertRoundTripEqual(value);
	}
}
