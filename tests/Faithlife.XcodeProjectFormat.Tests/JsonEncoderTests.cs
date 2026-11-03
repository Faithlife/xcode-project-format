using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class JsonEncoderTests
{
	[Test]
	public void Nulls() => ExpectEncoding(null, XCJsonValue.Null);

	[Test]
	public void Primitives()
	{
		ExpectEncoding(false, false);
		ExpectEncoding(true, true);
		ExpectEncoding(0, 0L);
		ExpectEncoding(1, 1L);
		ExpectEncoding(-1, -1L);
		ExpectEncoding(0.0, 0.0);
		ExpectEncoding(1.5, 1.5);
		ExpectEncoding(-1.5, -1.5);
		ExpectEncoding("", "");
		ExpectEncoding("A", "A");
	}

	[Test]
	public void Arrays()
	{
		ExpectEncoding(new[] { false }, XCJsonValue.FromArray(false));
		ExpectEncoding(new[] { false, true }, XCJsonValue.FromArray(false, true));
	}

	[Test]
	public void Dictionaries()
	{
		ExpectEncoding(new Dictionary<string, bool> { ["off"] = false }, XCJsonValue.FromObject(new XCJsonFieldOrComment("off", false)));
		ExpectEncoding(new Dictionary<string, bool> { ["on"] = true, ["off"] = false }, XCJsonValue.FromObject(new XCJsonFieldOrComment("off", false), new XCJsonFieldOrComment("on", true)));
	}

	[Test]
	public void Objects()
	{
		var makeThemAllPass = new TestTask(1234, 1.0, null, false, "Make sure they all pass", []);
		var addComments = new TestTask(12345, 4.5, "Documentation | X", true, "Add lots of comments", []);
		var umbrella = new TestTask(123, 1.25, null, false, "Write Encoder Tests", [makeThemAllPass, addComments]);

		var makeThemAllPassJson = XCJsonValue.FromObject(
			new XCJsonFieldOrComment("identifier", 1234L),
			new XCJsonFieldOrComment("priority", 1.0),
			new XCJsonFieldOrComment("title", "Make sure they all pass"));
		var addCommentsJson = XCJsonValue.FromObject(
			new XCJsonFieldOrComment("identifier", 12345L),
			new XCJsonFieldOrComment("priority", 4.5),
			new XCJsonFieldOrComment("component", "Documentation | X"),
			new XCJsonFieldOrComment("optional", true),
			new XCJsonFieldOrComment("title", "Add lots of comments"));

		ExpectEncoding(makeThemAllPass, makeThemAllPassJson);
		ExpectEncoding(addComments, addCommentsJson);
		ExpectEncoding(umbrella, XCJsonValue.FromObject(
			new XCJsonFieldOrComment("identifier", 123L),
			new XCJsonFieldOrComment("priority", 1.25),
			new XCJsonFieldOrComment("title", "Write Encoder Tests"),
			new XCJsonFieldOrComment("subtasks", XCJsonValue.FromArray(makeThemAllPassJson, addCommentsJson))));
	}

	private static void ExpectEncoding(object? value, XCJsonValue expected) => Assert.That(XCJsonEncoder.EncodeToValue(value), Is.EqualTo(expected));

	private sealed record TestTask(long Identifier, double Priority, string? Component, bool Optional, string Title, List<TestTask> Subtasks) : IXCJsonEncodable
	{
		void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
		{
			var container = encoder.OpenKeyedContainer();
			container.Encode("identifier", Identifier);
			container.Encode("priority", Priority);
			container.EncodeIfNotNull("component", Component);
			container.EncodeIfNotDefault("optional", Optional, false);
			container.Encode("title", Title);
			container.EncodeIfNotEmpty("subtasks", Subtasks);
		}
	}
}
