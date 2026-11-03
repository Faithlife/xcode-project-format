using System.Globalization;
using System.Text;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Parses JSON5 (https://spec.json5.org/), which is a superset of JSON. Comments are discarded.
/// </summary>
internal sealed class XCJsonParser
{
	public static XCJsonValue Parse(string text)
	{
		var parser = new XCJsonParser(text);
		parser.SkipWhitespaceAndComments();
		var value = parser.ParseValue(0);
		parser.SkipWhitespaceAndComments();
		if (parser.m_index != text.Length)
			throw parser.CreateException("Unexpected content after the root value");
		return value;
	}

	private XCJsonParser(string text) => m_text = text;

	private XCJsonValue ParseValue(int depth)
	{
		if (depth > c_maximumDepth)
			throw CreateException("Too many nested arrays or objects");
		if (m_index >= m_text.Length)
			throw CreateException("Unexpected end of input");

		var ch = m_text[m_index];
		switch (ch)
		{
			case '{':
				return ParseObject(depth);
			case '[':
				return ParseArray(depth);
			case '"':
			case '\'':
				return XCJsonValue.FromString(ParseString());
			case 'n':
				ExpectKeyword("null");
				return XCJsonValue.Null;
			case 't':
				ExpectKeyword("true");
				return XCJsonValue.True;
			case 'f':
				ExpectKeyword("false");
				return XCJsonValue.False;
			default:
				if (ch is '-' or '+' or '.' or 'I' or 'N' || IsDecimalDigit(ch))
					return ParseNumber();
				throw CreateException($"Unexpected character '{ch}'");
		}
	}

	private XCJsonValue ParseObject(int depth)
	{
		m_index++;
		var entries = new List<XCJsonFieldOrComment>();
		while (true)
		{
			SkipWhitespaceAndComments();
			if (TryConsume('}'))
				return XCJsonValue.FromObject(entries);

			var key = ParseKey();
			SkipWhitespaceAndComments();
			if (!TryConsume(':'))
				throw CreateException("Expected ':' after object key");
			SkipWhitespaceAndComments();
			entries.Add(new XCJsonFieldOrComment(key, ParseValue(depth + 1)));
			SkipWhitespaceAndComments();
			if (TryConsume('}'))
				return XCJsonValue.FromObject(entries);
			if (!TryConsume(','))
				throw CreateException("Expected ',' or '}' in object");
		}
	}

	private XCJsonValue ParseArray(int depth)
	{
		m_index++;
		var entries = new List<XCJsonValueOrComment>();
		while (true)
		{
			SkipWhitespaceAndComments();
			if (TryConsume(']'))
				return XCJsonValue.FromArray(entries);

			entries.Add(ParseValue(depth + 1));
			SkipWhitespaceAndComments();
			if (TryConsume(']'))
				return XCJsonValue.FromArray(entries);
			if (!TryConsume(','))
				throw CreateException("Expected ',' or ']' in array");
		}
	}

	private string ParseKey()
	{
		if (m_index >= m_text.Length)
			throw CreateException("Unexpected end of input");

		var ch = m_text[m_index];
		if (ch is '"' or '\'')
			return ParseString();

		var builder = new StringBuilder();
		while (m_index < m_text.Length)
		{
			ch = m_text[m_index];
			if (ch == '\\')
			{
				m_index++;
				if (!TryConsume('u'))
					throw CreateException("Invalid escape sequence in identifier");
				ch = ParseHexCharacter(4);
				if (!IsIdentifierCharacter(ch, isFirst: builder.Length == 0))
					throw CreateException("Invalid character in identifier");
				builder.Append(ch);
			}
			else if (char.IsSurrogatePair(m_text, m_index))
			{
				var category = char.GetUnicodeCategory(m_text, m_index);
				if (!IsIdentifierCategory(category, isFirst: builder.Length == 0))
					break;
				builder.Append(m_text, m_index, 2);
				m_index += 2;
			}
			else if (IsIdentifierCharacter(ch, isFirst: builder.Length == 0))
			{
				builder.Append(ch);
				m_index++;
			}
			else
			{
				break;
			}
		}

		if (builder.Length == 0)
			throw CreateException("Expected an object key");
		return builder.ToString();
	}

	private static bool IsIdentifierCharacter(char ch, bool isFirst) =>
		ch is '$' or '_' || (!char.IsSurrogate(ch) && IsIdentifierCategory(char.GetUnicodeCategory(ch), isFirst)) || (!isFirst && ch is '\u200C' or '\u200D');

	private static bool IsIdentifierCategory(UnicodeCategory category, bool isFirst) => category switch
	{
		UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or
			UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber => true,
		UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.DecimalDigitNumber or
			UnicodeCategory.ConnectorPunctuation => !isFirst,
		_ => false,
	};

	private string ParseString()
	{
		var quote = m_text[m_index++];
		var builder = new StringBuilder();
		while (true)
		{
			if (m_index >= m_text.Length)
				throw CreateException("Unterminated string");

			var ch = m_text[m_index++];
			if (ch == quote)
				return builder.ToString();

			if (ch == '\\')
			{
				if (m_index >= m_text.Length)
					throw CreateException("Unterminated string");

				var escaped = m_text[m_index++];
				switch (escaped)
				{
					case 'b':
						builder.Append('\b');
						break;
					case 'f':
						builder.Append('\f');
						break;
					case 'n':
						builder.Append('\n');
						break;
					case 'r':
						builder.Append('\r');
						break;
					case 't':
						builder.Append('\t');
						break;
					case 'v':
						builder.Append('\v');
						break;
					case '0':
						if (m_index < m_text.Length && IsDecimalDigit(m_text[m_index]))
							throw CreateException("Invalid escape sequence");
						builder.Append('\0');
						break;
					case 'x':
						builder.Append(ParseHexCharacter(2));
						break;
					case 'u':
						builder.Append(ParseHexCharacter(4));
						break;
					case '\r':
						TryConsume('\n');
						break;
					case '\n':
					case '\u2028':
					case '\u2029':
						break;
					case >= '1' and <= '9':
						throw CreateException("Invalid escape sequence");
					default:
						builder.Append(escaped);
						break;
				}
			}
			else if (ch is '\n' or '\r')
			{
				throw CreateException("Unescaped line terminator in string");
			}
			else
			{
				builder.Append(ch);
			}
		}
	}

	private char ParseHexCharacter(int digitCount)
	{
		if (m_index + digitCount > m_text.Length ||
			!int.TryParse(m_text.AsSpan(m_index, digitCount), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var value))
		{
			throw CreateException("Invalid hexadecimal escape sequence");
		}

		m_index += digitCount;
		return (char) value;
	}

	private XCJsonValue ParseNumber()
	{
		var start = m_index;
		var isNegative = false;
		if (m_text[m_index] is '+' or '-')
			isNegative = m_text[m_index++] == '-';

		if (TryConsumeKeyword("Infinity"))
			return XCJsonValue.FromDouble(isNegative ? double.NegativeInfinity : double.PositiveInfinity);
		if (TryConsumeKeyword("NaN"))
			return XCJsonValue.FromDouble(double.NaN);

		if (m_index + 1 < m_text.Length && m_text[m_index] == '0' && m_text[m_index + 1] is 'x' or 'X')
		{
			m_index += 2;
			var hexStart = m_index;
			while (m_index < m_text.Length && IsHexDigit(m_text[m_index]))
				m_index++;
			if (m_index == hexStart)
				throw CreateException("Invalid hexadecimal number");
			EnsureNumberTerminated();

			var hexDigits = m_text.AsSpan(hexStart, m_index - hexStart);
			if (ulong.TryParse(hexDigits, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var hexValue) && hexValue <= long.MaxValue)
				return XCJsonValue.FromInteger(isNegative ? -(long) hexValue : (long) hexValue);
			var bigValue = System.Numerics.BigInteger.Parse("0" + hexDigits.ToString(), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
			return XCJsonValue.FromDouble(isNegative ? -(double) bigValue : (double) bigValue);
		}

		var integerDigitCount = ConsumeDecimalDigits();
		if (integerDigitCount > 1 && m_text[m_index - integerDigitCount] == '0')
			throw CreateException("Numbers cannot have leading zeros");

		var isInteger = true;
		var fractionDigitCount = 0;
		if (TryConsume('.'))
		{
			isInteger = false;
			fractionDigitCount = ConsumeDecimalDigits();
		}
		if (integerDigitCount == 0 && fractionDigitCount == 0)
			throw CreateException("Invalid number");

		if (m_index < m_text.Length && m_text[m_index] is 'e' or 'E')
		{
			isInteger = false;
			m_index++;
			if (m_index < m_text.Length && m_text[m_index] is '+' or '-')
				m_index++;
			if (ConsumeDecimalDigits() == 0)
				throw CreateException("Invalid number exponent");
		}
		EnsureNumberTerminated();

		var span = m_text.AsSpan(start, m_index - start);
		if (span[0] == '+')
			span = span[1..];

		if (isInteger && long.TryParse(span, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var integerValue))
			return XCJsonValue.FromInteger(integerValue);
		return XCJsonValue.FromDouble(double.Parse(span, NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	private int ConsumeDecimalDigits()
	{
		var start = m_index;
		while (m_index < m_text.Length && IsDecimalDigit(m_text[m_index]))
			m_index++;
		return m_index - start;
	}

	private void EnsureNumberTerminated()
	{
		if (m_index < m_text.Length && (char.IsLetterOrDigit(m_text[m_index]) || m_text[m_index] is '_' or '$' or '.'))
			throw CreateException("Invalid number");
	}

	private void ExpectKeyword(string keyword)
	{
		if (!TryConsumeKeyword(keyword))
			throw CreateException("Invalid literal");
	}

	private bool TryConsumeKeyword(string keyword)
	{
		if (string.CompareOrdinal(m_text, m_index, keyword, 0, keyword.Length) != 0)
			return false;
		var end = m_index + keyword.Length;
		if (end < m_text.Length && IsIdentifierCharacter(m_text[end], isFirst: false))
			return false;
		m_index = end;
		return true;
	}

	private bool TryConsume(char ch)
	{
		if (m_index < m_text.Length && m_text[m_index] == ch)
		{
			m_index++;
			return true;
		}
		return false;
	}

	private void SkipWhitespaceAndComments()
	{
		while (m_index < m_text.Length)
		{
			var ch = m_text[m_index];
			if (ch is '\t' or '\n' or '\v' or '\f' or '\r' or ' ' or '\u00A0' or '\u2028' or '\u2029' or '\uFEFF' ||
				char.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator)
			{
				m_index++;
			}
			else if (ch == '/' && m_index + 1 < m_text.Length && m_text[m_index + 1] == '/')
			{
				m_index += 2;
				while (m_index < m_text.Length && m_text[m_index] is not ('\n' or '\r' or '\u2028' or '\u2029'))
					m_index++;
			}
			else if (ch == '/' && m_index + 1 < m_text.Length && m_text[m_index + 1] == '*')
			{
				var end = m_text.IndexOf("*/", m_index + 2, StringComparison.Ordinal);
				if (end < 0)
					throw CreateException("Unterminated comment");
				m_index = end + 2;
			}
			else
			{
				break;
			}
		}
	}

	private static bool IsDecimalDigit(char ch) => ch is >= '0' and <= '9';

	private static bool IsHexDigit(char ch) => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

	private XcodeProjectFormatException CreateException(string message)
	{
		var line = 1;
		var column = 1;
		for (var index = 0; index < m_index && index < m_text.Length; index++)
		{
			if (m_text[index] == '\n')
			{
				line++;
				column = 1;
			}
			else
			{
				column++;
			}
		}
		return new XcodeProjectFormatException($"The data couldn’t be read because it isn’t in the correct format. {message} at line {line.ToString(CultureInfo.InvariantCulture)}, column {column.ToString(CultureInfo.InvariantCulture)}.");
	}

	private const int c_maximumDepth = 512;

	private readonly string m_text;
	private int m_index;
}
