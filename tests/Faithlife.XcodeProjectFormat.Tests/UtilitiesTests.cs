using Faithlife.XcodeProjectFormat.Schema;
using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class UtilitiesTests
{
	[TestCase("", "")]
	[TestCase("a", "a")]
	[TestCase("<", "\\<")]
	[TestCase("\\", "\\\\")]
	[TestCase("<<", "\\<\\<")]
	public void Escaping(string original, string expectedEncoding)
	{
		var actualEncoding = original.EscapingCharacter('<');
		Assert.That(actualEncoding, Is.EqualTo(expectedEncoding));
		Assert.That(actualEncoding.UnescapingCharacter('<'), Is.EqualTo(original));
	}

	[TestCase("\\<")]
	[TestCase("\\")]
	public void InvalidUnescaping(string value) => Assert.Throws<XcodeProjectFormatException>(() => value.UnescapingCharacter('j'));

	[TestCase("a", "b", -1)]
	[TestCase("b", "b", 0)]
	[TestCase("b", "a", 1)]
	[TestCase("\uFFFF", "\U0001F600", -1)]
	[TestCase(null, "a", -1)]
	[TestCase("a", null, 1)]
	[TestCase(null, null, 0)]
	public void ComparisonOperators(string? left, string? right, int expectedSign)
	{
		var leftId = left is null ? null : new ObjectId(left);
		var rightId = right is null ? null : new ObjectId(right);
		Assert.That(
			(leftId < rightId, leftId <= rightId, leftId > rightId, leftId >= rightId),
			Is.EqualTo((expectedSign < 0, expectedSign <= 0, expectedSign > 0, expectedSign >= 0)));
	}
}
