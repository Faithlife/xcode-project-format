using System.Collections;
using System.Reflection;
using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Tests;

internal static class TestUtilities
{
	public static void AssertRoundTripEqual<T>(T value)
	{
		var text = XCJsonEncoder.EncodeToText(value);
		T decoded;
		try
		{
			decoded = XCJsonDecoder.DecodeText<T>(text);
		}
		catch (XcodeProjectFormatException exception)
		{
			Assert.Fail($"Decoding failed: {exception.Message}\n{text}");
			throw;
		}

		var difference = FindDifference(value, decoded, "");
		Assert.That(difference, Is.Null, $"Round trip of {typeof(T).Name} differs at {difference}:\n{text}");
		Assert.That(XCJsonEncoder.EncodeToText(decoded), Is.EqualTo(text));
	}

	public static string StringFromLines(params string[] lines) => string.Join("\n", lines) + "\n";

	public static IEnumerable<IReadOnlyList<T>> EnumeratePermutations<T>(IReadOnlyList<T> items)
	{
		var permutation = items.ToArray();
		return Shuffle(0);

		IEnumerable<IReadOnlyList<T>> Shuffle(int decidedLength)
		{
			if (decidedLength == permutation.Length)
			{
				yield return permutation.ToArray();
			}
			else
			{
				for (var index = decidedLength; index < permutation.Length; index++)
				{
					(permutation[decidedLength], permutation[index]) = (permutation[index], permutation[decidedLength]);
					foreach (var result in Shuffle(decidedLength + 1))
						yield return result;
					(permutation[decidedLength], permutation[index]) = (permutation[index], permutation[decidedLength]);
				}
			}
		}
	}

	public static IEnumerable<IReadOnlyList<T>> EnumerateSubsets<T>(IReadOnlyList<T> items)
	{
		var subset = new List<T>();
		yield return subset.ToArray();
		foreach (var result in Enumerate(0))
			yield return result;

		IEnumerable<IReadOnlyList<T>> Enumerate(int position)
		{
			if (position != items.Count)
			{
				subset.Add(items[position]);
				yield return subset.ToArray();
				foreach (var result in Enumerate(position + 1))
					yield return result;
				subset.RemoveAt(subset.Count - 1);
				foreach (var result in Enumerate(position + 1))
					yield return result;
			}
		}
	}

	public static IEnumerable<IReadOnlyList<T>> EnumerateSubsetPermutations<T>(IReadOnlyList<T> items) =>
		EnumerateSubsets(items).SelectMany(EnumeratePermutations);

	public static IEnumerable<string> AllSubsetPermutationJoinings(IReadOnlyList<string> items) =>
		EnumerateSubsetPermutations(items).Select(x => string.Concat(x));

	// Returns the path of the first difference between two values, comparing mutable objects by their public properties.
	public static string? FindDifference(object? left, object? right, string path)
	{
		if (left is null || right is null)
			return left is null && right is null ? null : path;
		if (left.GetType() != right.GetType())
			return path + " (type)";

		var type = left.GetType();
		if (type.IsPrimitive || type.IsEnum || left is string || left is decimal || ImplementsEquatableOfSelf(type))
			return Equals(left, right) ? null : path;

		if (left is IDictionary leftDictionary)
		{
			var rightDictionary = (IDictionary) right;
			if (leftDictionary.Count != rightDictionary.Count)
				return path + " (count)";
			foreach (DictionaryEntry entry in leftDictionary)
			{
				if (!rightDictionary.Contains(entry.Key))
					return $"{path}[{entry.Key}]";
				if (FindDifference(entry.Value, rightDictionary[entry.Key], $"{path}[{entry.Key}]") is { } difference)
					return difference;
			}
			return null;
		}

		if (IsSet(type))
		{
			var leftItems = ((IEnumerable) left).Cast<object>().ToList();
			var rightItems = ((IEnumerable) right).Cast<object>().ToList();
			return leftItems.Count == rightItems.Count && leftItems.All(x => rightItems.Any(y => FindDifference(x, y, "") is null)) ? null : path;
		}

		if (left is IEnumerable leftEnumerable)
		{
			var leftItems = leftEnumerable.Cast<object?>().ToList();
			var rightItems = ((IEnumerable) right).Cast<object?>().ToList();
			if (leftItems.Count != rightItems.Count)
				return path + " (count)";
			for (var index = 0; index < leftItems.Count; index++)
			{
				if (FindDifference(leftItems[index], rightItems[index], $"{path}[{index}]") is { } difference)
					return difference;
			}
			return null;
		}

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length != 0)
				continue;
			if (FindDifference(property.GetValue(left), property.GetValue(right), path + "." + property.Name) is { } difference)
				return difference;
		}
		return null;
	}

	private static bool ImplementsEquatableOfSelf(Type type) => type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEquatable<>) && x.GetGenericArguments()[0] == type);

	private static bool IsSet(Type type) => type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISet<>));
}
