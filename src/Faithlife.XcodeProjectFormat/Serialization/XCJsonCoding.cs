using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Faithlife.XcodeProjectFormat.Serialization;

internal interface IXCJsonEncodable
{
	void Encode(XCJsonEncoder encoder);
}

internal interface IXCJsonDecodable<TSelf>
	where TSelf : IXCJsonDecodable<TSelf>
{
	abstract static TSelf Decode(XCJsonDecoder decoder);
}

/// <summary>
/// A type whose properties are encoded as fields of a keyed container, which may be shared with an enclosing type.
/// </summary>
internal interface IXCJsonInlineKeyedEncodable : IXCJsonEncodable
{
	void Encode(XCJsonEncoder.KeyedContainer container);

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => Encode(encoder.OpenKeyedContainer());
}

internal interface IXCJsonInlineKeyedDecodable<TSelf> : IXCJsonDecodable<TSelf>
	where TSelf : IXCJsonInlineKeyedDecodable<TSelf>
{
	abstract static TSelf Decode(XCJsonDecoder.KeyedContainer container);

	static TSelf IXCJsonDecodable<TSelf>.Decode(XCJsonDecoder decoder) => TSelf.Decode(decoder.OpenKeyedContainer());
}

/// <summary>
/// A type that is encoded as a JSON string (and can be used as a dictionary key).
/// </summary>
internal interface IXCJsonStringEncodable : IXCJsonEncodable
{
	string EncodableStringRepresentation { get; }

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => encoder.OpenPrimitiveContainer().Encode(EncodableStringRepresentation);
}

internal interface IXCJsonStringCodable<TSelf> : IXCJsonStringEncodable, IXCJsonDecodable<TSelf>
	where TSelf : IXCJsonStringCodable<TSelf>
{
	abstract static TSelf FromEncodableStringRepresentation(string value);

	static TSelf IXCJsonDecodable<TSelf>.Decode(XCJsonDecoder decoder) => TSelf.FromEncodableStringRepresentation(decoder.DecodePrimitiveString());
}

internal static class XCJsonCoding
{
	public static XCJsonCompactArray WithCompactValueEncoding(this IEnumerable elements) => new(elements);

	public static XCJsonCompactDictionary WithCompactValueEncoding(this IDictionary dictionary) => new(dictionary);

	/// <summary>
	/// Compares strings by Unicode scalar value, which matches Swift's ordering of (normalized) strings.
	/// </summary>
	public static readonly IComparer<string> StringComparer = Comparer<string>.Create(CompareStrings);

	public static int CompareStrings(string? left, string? right)
	{
		if (left is null || right is null)
			return left is null ? (right is null ? 0 : -1) : 1;

		var leftRunes = left.EnumerateRunes();
		var rightRunes = right.EnumerateRunes();
		while (true)
		{
			var hasLeft = leftRunes.MoveNext();
			var hasRight = rightRunes.MoveNext();
			if (!hasLeft || !hasRight)
				return hasLeft ? 1 : hasRight ? -1 : 0;
			var comparison = leftRunes.Current.Value.CompareTo(rightRunes.Current.Value);
			if (comparison != 0)
				return comparison;
		}
	}

	public static string SmartQuoted(this string value) => "“" + value + "”";

	public static string SmartQuoted(this object? value) => SmartQuoted(value?.ToString() ?? "");

	public static bool IsSpearCase(string key) => key.Length != 0 && key[0] is >= 'a' and <= 'z' && key.All(x => x is >= 'a' and <= 'z' or '-');

	public static string GetEnumName<T>(T value)
		where T : struct, Enum => EnumNames<T>.ToName.TryGetValue(value, out var name) ? name : throw new ArgumentOutOfRangeException(nameof(value), value, null);

	public static T ParseEnumName<T>(string name)
		where T : struct, Enum => EnumNames<T>.FromName.TryGetValue(name, out var value) ? value : throw new XcodeProjectFormatException($"Unexpected value {name.SmartQuoted()}");

	public static bool TryParseEnumName<T>(string name, out T value)
		where T : struct, Enum => EnumNames<T>.FromName.TryGetValue(name, out value);

	public static string GetEnumName(Enum value) => GetEnumNameMap(value.GetType()).ToName[value];

	public static object ParseEnumName(Type enumType, string name) =>
		GetEnumNameMap(enumType).FromName.TryGetValue(name, out var value) ? value : throw new XcodeProjectFormatException($"Unexpected value {name.SmartQuoted()}");

	/// <summary>
	/// Returns the string used to represent the specified value as a JSON dictionary key.
	/// </summary>
	public static string GetKeyString(object key) => key switch
	{
		string text => text,
		IXCJsonStringEncodable encodable => encodable.EncodableStringRepresentation,
		Enum value => GetEnumName(value),
		_ => throw new ArgumentException($"Type {key.GetType()} can't be used as a JSON dictionary key.", nameof(key)),
	};

	public static void EncodeDictionary(XCJsonEncoder encoder, IDictionary dictionary, XCJsonPrintingDensity? density)
	{
		var container = encoder.OpenKeyedContainer();
		var entries = new List<KeyValuePair<string, object?>>();
		foreach (DictionaryEntry entry in dictionary)
			entries.Add(new KeyValuePair<string, object?>(GetKeyString(entry.Key), entry.Value));
		entries.Sort((left, right) => CompareStrings(left.Key, right.Key));
		foreach (var entry in entries)
			container.EncodeUnverified(entry.Key, entry.Value, density);
	}

	/// <summary>
	/// Returns the elements of a collection in encoding order. Unordered sets are sorted (by their string representation, if any).
	/// </summary>
	public static IEnumerable GetOrderedElements(IEnumerable collection)
	{
		if (!IsSet(collection.GetType()))
			return collection;

		var elements = collection.Cast<object>().ToList();
		elements.Sort(CompareSetElements);
		return elements;
	}

	public static bool IsSet(Type type) => type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISet<>));

	private static int CompareSetElements(object? left, object? right)
	{
		if (left is string or IXCJsonStringEncodable or Enum && right is string or IXCJsonStringEncodable or Enum)
			return CompareStrings(GetKeyString(left), GetKeyString(right));
		return Comparer<object>.Default.Compare(left, right);
	}

	/// <summary>
	/// Determines whether two values are structurally equal; collections are compared by content.
	/// </summary>
	public static bool AreEqual(object? left, object? right)
	{
		if (ReferenceEquals(left, right))
			return true;
		if (left is null || right is null)
			return false;
		if (left is string || right is string)
			return left.Equals(right);

		if (left is IDictionary leftDictionary && right is IDictionary rightDictionary)
		{
			if (leftDictionary.Count != rightDictionary.Count)
				return false;
			foreach (DictionaryEntry entry in leftDictionary)
			{
				if (!rightDictionary.Contains(entry.Key) || !AreEqual(entry.Value, rightDictionary[entry.Key]))
					return false;
			}
			return true;
		}

		if (left is IEnumerable leftEnumerable && right is IEnumerable rightEnumerable)
		{
			var leftElements = leftEnumerable.Cast<object?>().ToList();
			var rightElements = rightEnumerable.Cast<object?>().ToList();
			if (leftElements.Count != rightElements.Count)
				return false;
			if (IsSet(left.GetType()))
				return leftElements.All(rightElements.Contains);
			return leftElements.Zip(rightElements).All(x => AreEqual(x.First, x.Second));
		}

		return left.Equals(right);
	}

	/// <summary>
	/// Determines whether the specified value is null or an empty collection.
	/// </summary>
	public static bool IsNullOrEmpty(object? value) => value switch
	{
		null => true,
		string => false,
		ICollection collection => collection.Count == 0,
		IEnumerable enumerable => !enumerable.GetEnumerator().MoveNext(),
		_ => false,
	};

	public static string EscapingCharacter(this string value, char escaped)
	{
		var output = new StringBuilder(value.Length);
		var index = 0;
		while (index < value.Length)
		{
			var length = GetTextElementLength(value, index);
			if (length == 1 && (value[index] == escaped || value[index] == '\\'))
				output.Append('\\');
			output.Append(value, index, length);
			index += length;
		}
		return output.ToString();
	}

	public static string UnescapingCharacter(this string value, char escaped)
	{
		var result = UnescapeUntilError(value, escaped, out var remaining, out var error);
		return error switch
		{
			UnescapeError.None => result,
			UnescapeError.UnescapedSequence => throw new XcodeProjectFormatException($"Missing escape sequence for {escaped.ToString().SmartQuoted()} in {value.SmartQuoted()}"),
			UnescapeError.InvalidEscapeSequence => throw new XcodeProjectFormatException($"Invalid escape sequence {("\\" + remaining).SmartQuoted()} in {value.SmartQuoted()}"),
			_ => throw new XcodeProjectFormatException($"Invalid escape sequence in {value.SmartQuoted()}"),
		};
	}

	public enum UnescapeError
	{
		None,
		UnescapedSequence,
		InvalidEscapeSequence,
		UnresolvedEscape,
	}

	/// <summary>
	/// Unescapes the specified string until an error occurs. If an unescaped <paramref name="escaped"/> is found, the
	/// unescaped prefix is returned, and <paramref name="remaining"/> is set to the text following that character. If an
	/// invalid escape sequence is found, <paramref name="remaining"/> is set to the invalid escaped character.
	/// </summary>
	public static string UnescapeUntilError(string value, char escaped, out string remaining, out UnescapeError error)
	{
		remaining = "";
		error = UnescapeError.None;
		if (value.IndexOfAny([escaped, '\\']) < 0)
			return value;

		var output = new StringBuilder(value.Length);
		var index = 0;
		while (index < value.Length)
		{
			var length = GetTextElementLength(value, index);
			var ch = value[index];
			if (length == 1 && ch == '\\')
			{
				index++;
				if (index == value.Length)
				{
					error = UnescapeError.UnresolvedEscape;
					return output.ToString();
				}

				var nextLength = GetTextElementLength(value, index);
				var next = value[index];
				if (nextLength == 1 && (next == escaped || next == '\\'))
				{
					output.Append(next);
				}
				else
				{
					error = UnescapeError.InvalidEscapeSequence;
					remaining = value.Substring(index, nextLength);
					return output.ToString();
				}
				index += nextLength;
			}
			else if (length == 1 && ch == escaped)
			{
				error = UnescapeError.UnescapedSequence;
				remaining = value[(index + 1)..];
				return output.ToString();
			}
			else
			{
				output.Append(value, index, length);
				index += length;
			}
		}
		return output.ToString();
	}

	// Swift compares Characters (grapheme clusters), so an escape character combined with a following mark isn't an escape character
	private static int GetTextElementLength(string value, int index) => System.Globalization.StringInfo.GetNextTextElementLength(value.AsSpan(index));

	private static (Dictionary<Enum, string> ToName, Dictionary<string, Enum> FromName) GetEnumNameMap(Type enumType) =>
		s_enumNameMaps.GetOrAdd(enumType, static type =>
		{
			var toName = new Dictionary<Enum, string>();
			var fromName = new Dictionary<string, Enum>(System.StringComparer.Ordinal);
			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var name = field.GetCustomAttribute<XCJsonNameAttribute>()?.Name ?? throw new InvalidOperationException($"{type.Name}.{field.Name} is missing an XCJsonName attribute.");
				var value = (Enum) field.GetValue(null)!;
				toName[value] = name;
				fromName[name] = value;
			}
			return (toName, fromName);
		});

	private static class EnumNames<T>
		where T : struct, Enum
	{
		public static readonly Dictionary<T, string> ToName = GetEnumNameMap(typeof(T)).ToName.ToDictionary(x => (T) x.Key, x => x.Value);
		public static readonly Dictionary<string, T> FromName = GetEnumNameMap(typeof(T)).FromName.ToDictionary(x => x.Key, x => (T) x.Value, System.StringComparer.Ordinal);
	}

	private static readonly ConcurrentDictionary<Type, (Dictionary<Enum, string> ToName, Dictionary<string, Enum> FromName)> s_enumNameMaps = new();
}
