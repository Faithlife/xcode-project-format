using Faithlife.XcodeProjectFormat.Schema;
using static Faithlife.XcodeProjectFormat.Tests.TestUtilities;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class StringEscapingDuringEncodingTests
{
	[Test]
	public void StringEncoding()
	{
		TextEncoding[] encodings =
		[
			TextEncoding.Ascii,
			TextEncoding.NextStep,
			TextEncoding.JapaneseEuc,
			TextEncoding.Utf8,
			TextEncoding.IsoLatin1,
			TextEncoding.Symbol,
			TextEncoding.NonLossyAscii,
			TextEncoding.ShiftJis,
			TextEncoding.IsoLatin2,
			TextEncoding.Unicode,
			TextEncoding.WindowsCodePage1251,
			TextEncoding.WindowsCodePage1252,
			TextEncoding.WindowsCodePage1253,
			TextEncoding.WindowsCodePage1254,
			TextEncoding.WindowsCodePage1250,
			TextEncoding.Iso2022Jp,
			TextEncoding.MacOSRoman,
			TextEncoding.Utf16,
			TextEncoding.Utf16BigEndian,
			TextEncoding.Utf16LittleEndian,
			TextEncoding.Utf32,
			TextEncoding.Utf32BigEndian,
			TextEncoding.Utf32LittleEndian,
			new TextEncoding(23418341), // unknown values are persisted
		];
		foreach (var encoding in encodings)
			AssertRoundTripEqual(encoding);
	}

	[Test]
	public void BuildPhaseReference()
	{
		string[] hardStrings = ["", "/", "a", "b"];
		AssertRoundTripEqual(new TargetBuildPhaseReference(BuildPhaseKind.Copy));
		foreach (var hardString in AllSubsetPermutationJoinings(hardStrings))
			AssertRoundTripEqual(new TargetBuildPhaseReference(BuildPhaseKind.Copy, hardString));

		foreach (var targetName in AllSubsetPermutationJoinings(hardStrings))
		{
			AssertRoundTripEqual(new ProjectBuildPhaseReference(new LocalTargetReference(targetName), BuildPhaseKind.Copy));
			foreach (var buildPhaseName in AllSubsetPermutationJoinings(hardStrings))
				AssertRoundTripEqual(new ProjectBuildPhaseReference(new LocalTargetReference(targetName), BuildPhaseKind.Copy, buildPhaseName));
		}
	}

	[Test]
	public void SwiftPackageVersionConstraints()
	{
		foreach (var hardString in AllSubsetPermutationJoinings([".", "..", "1", "<", "2"]))
		{
			AssertRoundTripEqual(SwiftPackageVersionConstraint.Revision(hardString));
			AssertRoundTripEqual(SwiftPackageVersionConstraint.Branch(hardString));
			AssertRoundTripEqual(SwiftPackageVersionConstraint.Version(hardString));
			AssertRoundTripEqual(SwiftPackageVersionConstraint.VersionRange(hardString, hardString));
			AssertRoundTripEqual(SwiftPackageVersionConstraint.UpToNextMinorVersion(hardString));
			AssertRoundTripEqual(SwiftPackageVersionConstraint.UpToNextMajorVersion(hardString));
		}
	}

	[Test]
	public void FilePaths()
	{
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("PRODUCTS"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot(""), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("$"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("$a"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("$()"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("$asdf"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.SourceRoot("$(asdf)"), ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.Project, ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.Project, "."));
		AssertRoundTripEqual(new FilePath(FilePathBase.Project, ".."));
		AssertRoundTripEqual(new FilePath(FilePathBase.Project, "./"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Absolute, "/"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Absolute, "/."));
		AssertRoundTripEqual(new FilePath(FilePathBase.Absolute, "/.."));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, ""));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, "$"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, "$a"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, "$()"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, "$asdf"));
		AssertRoundTripEqual(new FilePath(FilePathBase.Group, "$(asdf)"));
		foreach (var hardString in AllSubsetPermutationJoinings(["$", "(", ")", "a"]))
			AssertRoundTripEqual(new FilePath(FilePathBase.Group, hardString));
	}

	[Test]
	public void NamePaths()
	{
		foreach (var hardString in AllSubsetPermutationJoinings(["a", "", "/", "//", ".", ".."]))
			AssertRoundTripEqual(new NamePath(NamePathComponent.Child(hardString)));
	}
}
