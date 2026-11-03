using System.Text;
using Faithlife.XcodeProjectFormat.Schema;
using Faithlife.XcodeProjectFormat.Serialization;
using static Faithlife.XcodeProjectFormat.Tests.TestUtilities;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class JsonRoundTrippingTests
{
	[Test]
	public void RoundTripping()
	{
		Assert.That(XCJsonEncoder.EncodeToText(null), Is.EqualTo("null\n"));
		AssertRoundTripEqual(false);
		AssertRoundTripEqual(true);
		AssertRoundTripEqual(0L);
		AssertRoundTripEqual(+1L);
		AssertRoundTripEqual(-1L);
		AssertRoundTripEqual(+1.5);
		AssertRoundTripEqual(-1.5);
		AssertRoundTripEqual("");
		AssertRoundTripEqual("String");
		AssertRoundTripEqual(new List<bool> { false });
		AssertRoundTripEqual(new List<bool> { true });
		AssertRoundTripEqual(new List<long> { 0 });
		AssertRoundTripEqual(new List<long> { +1 });
		AssertRoundTripEqual(new List<long> { -1 });
		AssertRoundTripEqual(new List<double> { +1.5 });
		AssertRoundTripEqual(new List<double> { -1.5 });
		AssertRoundTripEqual(new List<string> { "" });
		AssertRoundTripEqual(new List<string> { "String" });
		AssertRoundTripEqual(new Dictionary<string, bool> { ["field"] = false });
		AssertRoundTripEqual(new Dictionary<string, bool> { ["field"] = true });
		AssertRoundTripEqual(new Dictionary<string, long> { ["field"] = 0 });
		AssertRoundTripEqual(new Dictionary<string, long> { ["field"] = +1 });
		AssertRoundTripEqual(new Dictionary<string, long> { ["field"] = -1 });
		AssertRoundTripEqual(new Dictionary<string, double> { ["field"] = +1.5 });
		AssertRoundTripEqual(new Dictionary<string, double> { ["field"] = -1.5 });
		AssertRoundTripEqual(new Dictionary<string, string> { ["field"] = "" });
		AssertRoundTripEqual(new Dictionary<string, string> { ["field"] = "String" });
	}

	[Test]
	public void AllAsciiCharacterStrings()
	{
		for (var ascii = 0; ascii < 127; ascii++)
			AssertRoundTripEqual(((char) ascii).ToString());
	}

	[TestCase(FilePathBaseKind.Group, null, "File.swift", "File.swift")]
	[TestCase(FilePathBaseKind.Absolute, null, "/File.swift", "/File.swift")]
	[TestCase(FilePathBaseKind.Project, null, "File.swift", "<PROJECT>/File.swift")]
	[TestCase(FilePathBaseKind.Sdk, null, "File.swift", "<SDK>/File.swift")]
	[TestCase(FilePathBaseKind.Developer, null, "File.swift", "<DEVELOPER>/File.swift")]
	[TestCase(FilePathBaseKind.BuildProducts, null, "File.swift", "<PRODUCTS>/File.swift")]
	[TestCase(FilePathBaseKind.SourceRoot, "SHARED", "File.swift", "<USER:SHARED>/File.swift")]
	[TestCase(FilePathBaseKind.Group, null, "<", "\\<")]
	[TestCase(FilePathBaseKind.Group, null, "\\<>/Component", "\\\\\\<>/Component")]
	[TestCase(FilePathBaseKind.Group, null, "\\", "\\\\")]
	[TestCase(FilePathBaseKind.Group, null, "<Readme.md", "\\<Readme.md")]
	[TestCase(FilePathBaseKind.Group, null, "\\Readme.md", "\\\\Readme.md")]
	[TestCase(FilePathBaseKind.Group, null, "./Readme.md", "./Readme.md")]
	[TestCase(FilePathBaseKind.SourceRoot, "custom", "Readme.md", "<USER:custom>/Readme.md")]
	[TestCase(FilePathBaseKind.SourceRoot, "<", "Readme.md", "<USER:<>/Readme.md")]
	[TestCase(FilePathBaseKind.SourceRoot, ">", "Readme.md", "<USER:\\>>/Readme.md")]
	[TestCase(FilePathBaseKind.SourceRoot, "$()", "Readme.md", "<USER:$()>/Readme.md")]
	public void FilePathEncoding(FilePathBaseKind baseKind, string? sourceRoot, string path, string expectedText)
	{
		var original = new FilePath(CreateBase(baseKind, sourceRoot), path);
		Assert.That(original.ToString(), Is.EqualTo(expectedText));
		Assert.That(new FilePath(expectedText), Is.EqualTo(original));
		AssertRoundTripEqual(original);
	}

	[Test]
	public void PrintArrays()
	{
		ExpectText(XCJsonValue.FromArray(), StringFromLines("[", "]"));
		ExpectText(XCJsonValue.FromArray(1L), StringFromLines("[", "  1,", "]"));
		ExpectText(XCJsonValue.FromArray(), StringFromLines("[]"), RootCompact);
		ExpectText(XCJsonValue.FromArray(1L), StringFromLines("[ 1 ]"), RootCompact);
	}

	[Test]
	public void PrintDictionaries()
	{
		ExpectText(XCJsonValue.FromObject(), StringFromLines("{", "}"));
		ExpectText(XCJsonValue.FromObject(new XCJsonFieldOrComment("key", "value")), StringFromLines("{", "  \"key\": \"value\",", "}"));
		ExpectText(XCJsonValue.FromObject(), StringFromLines("{}"), RootCompact);
		ExpectText(XCJsonValue.FromObject(new XCJsonFieldOrComment("key", "value")), StringFromLines("{ \"key\": \"value\" }"), RootCompact);
	}

	[Test]
	public void PrintComments()
	{
		ExpectText(XCJsonValue.FromArray(1L, LineComment("Empty")), StringFromLines("[", "  1,", "  // Empty", "]"));
		ExpectText(XCJsonValue.FromArray("a string", LineComment("Trailing")), StringFromLines("[", "  \"a string\",", "  // Trailing", "]"));
		ExpectText(XCJsonValue.FromArray(LineComment("Leading"), "a string"), StringFromLines("[", "  // Leading", "  \"a string\",", "]"));
		ExpectText(XCJsonValue.FromArray(BlockComment("Block")), StringFromLines("[", "  /* Block */", "]"));
		ExpectText(XCJsonValue.FromArray(BlockComment("\nTwo Line\nOutdented Block\n")), StringFromLines("[", "  /*", "  Two Line", "  Outdented Block", "  */", "]"));
		ExpectText(XCJsonValue.FromArray(BlockComment("Two Line\nBlock")), StringFromLines("[", "  /* Two Line", "     Block */", "]"));
	}

	[TestCase(0, 0, 0, "0.0")]
	[TestCase(0, 0, 1, "0.0.1")]
	[TestCase(1, 0, 0, "1.0")]
	[TestCase(1, 2, 0, "1.2")]
	[TestCase(1, 0, 3, "1.0.3")]
	public void MarketingVersionSpellings(int major, int minor, int update, string expected)
	{
		var version = new MarketingVersion(major, minor, update);
		Assert.That(((IXCJsonStringEncodable) version).EncodableStringRepresentation, Is.EqualTo(expected));
		Assert.That(MarketingVersion.Parse(expected), Is.EqualTo(version));
	}

	[Test]
	public void Uncompacting()
	{
		ExpectText(XCJsonValue.FromArray("Line1\nLine2"), StringFromLines("[ \"Line1\\nLine2\" ]"), RootCompact);
		ExpectText(XCJsonValue.FromObject(new XCJsonFieldOrComment("line1\nline2", "value")), StringFromLines("{", "  \"line1\\nline2\": \"value\",", "}"));
		ExpectText(XCJsonValue.FromArray(LineComment("Empty")), StringFromLines("[", "  // Empty", "]"), RootCompact);
		ExpectText(XCJsonValue.FromArray(BlockComment("Empty")), StringFromLines("[ /* Empty */ ]"), RootCompact);
		ExpectText(XCJsonValue.FromArray(BlockComment("Line1\nLine2")), StringFromLines("[", "  /* Line1", "     Line2 */", "]"), RootCompact);
	}

	[Test]
	public void ArrayWhitespace()
	{
		var value1 = XCJsonValue.FromObject(new XCJsonFieldOrComment("key", "value"));
		var value2 = XCJsonValue.FromObject(new XCJsonFieldOrComment("key", "value2"));

		ExpectText(XCJsonValue.FromArray(value1), StringFromLines("[", "  {", "    \"key\": \"value\",", "  },", "]"));
		ExpectText(XCJsonValue.FromArray(value1, value2), StringFromLines("[", "  {", "    \"key\": \"value\",", "  }, {", "    \"key\": \"value2\",", "  },", "]"));
		ExpectText(XCJsonValue.FromArray(value1, 3L, value2), StringFromLines("[", "  {", "    \"key\": \"value\",", "  },", "  3,", "  {", "    \"key\": \"value2\",", "  },", "]"));
		ExpectText(XCJsonValue.FromArray(value1, 3L), StringFromLines("[", "  {", "    \"key\": \"value\",", "  },", "  3,", "]"));
		ExpectText(XCJsonValue.FromArray(value1), StringFromLines("[", "  { \"key\": \"value\" },", "]"), CompactIndices(0));
		ExpectText(XCJsonValue.FromArray(value1, value2), StringFromLines("[", "  { \"key\": \"value\" },", "  { \"key\": \"value2\" },", "]"), CompactIndices(0, 1));
		ExpectText(XCJsonValue.FromArray(value1, 3L, value2), StringFromLines("[", "  { \"key\": \"value\" },", "  3,", "  { \"key\": \"value2\" },", "]"), CompactIndices(0, 2));
	}

	[Test]
	public void IntegerExtremes()
	{
		AssertRoundTripEqual(long.MinValue);
		AssertRoundTripEqual(0L);
		AssertRoundTripEqual(long.MaxValue);
	}

	[Test]
	public void FloatingPointExtremes()
	{
		AssertRoundTripEqual(double.MaxValue);
		AssertRoundTripEqual(Math.PI);
		AssertRoundTripEqual(double.Epsilon);
		AssertRoundTripEqual(Math.BitIncrement(1.0) - 1.0);
		AssertRoundTripEqual(float.MaxValue);
		AssertRoundTripEqual(MathF.PI);
		AssertRoundTripEqual(MathF.BitIncrement(1.0f) - 1.0f);
	}

	[Test]
	public void PrintsUtf8()
	{
		var value = XCJsonValue.FromArray([LineComment("\U0001F600 comment"), "\u00E9\u2603\U0001F600"]);
		Assert.That(value.ToUtf8(), Is.EqualTo(Encoding.UTF8.GetBytes("[\n  // \U0001F600 comment\n  \"\u00E9\u2603\U0001F600\",\n]\n")));
	}

	[Test]
	public void CommentLineSeparators() =>
		ExpectText(XCJsonValue.FromArray(LineComment("a\r\nb\u2028c\u2029")), "[\n  // a\r\nb\n  // c\n  // \n]\n");

	[Test]
	public void InvalidUtf16() => Assert.Throws<XcodeProjectFormatException>(() => XCJsonValue.FromString("\uD800").ToUtf8());

	private static FilePathBase CreateBase(FilePathBaseKind kind, string? sourceRoot) => kind switch
	{
		FilePathBaseKind.Absolute => FilePathBase.Absolute,
		FilePathBaseKind.Group => FilePathBase.Group,
		FilePathBaseKind.Project => FilePathBase.Project,
		FilePathBaseKind.Developer => FilePathBase.Developer,
		FilePathBaseKind.BuildProducts => FilePathBase.BuildProducts,
		FilePathBaseKind.Sdk => FilePathBase.Sdk,
		_ => FilePathBase.SourceRoot(sourceRoot!),
	};

	private static XCJsonComment LineComment(string content) => new(XCJsonCommentStyle.Line, content);

	private static XCJsonComment BlockComment(string content) => new(XCJsonCommentStyle.Block, content);

	private static Dictionary<XCJsonPath, XCJsonPrintingDensity> RootCompact => new() { [XCJsonPath.Root] = XCJsonPrintingDensity.Compact };

	private static Dictionary<XCJsonPath, XCJsonPrintingDensity> CompactIndices(params int[] indices) =>
		indices.ToDictionary(XCJsonPath.Root.Append, _ => XCJsonPrintingDensity.Compact);

	private static void ExpectText(XCJsonValue value, string expectedText, IReadOnlyDictionary<XCJsonPath, XCJsonPrintingDensity>? densities = null) =>
		Assert.That(value.ToText(XCJsonEncodingOptions.Default, densities), Is.EqualTo(expectedText));
}
