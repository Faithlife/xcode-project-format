using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Unicode;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Prints JSON in the format used by Xcode, as UTF-8.
/// </summary>
internal sealed class XCJsonPrinter
{
	public static byte[] PrintUtf8(XCJsonValue root, XCJsonEncodingOptions options, IReadOnlyDictionary<XCJsonPath, XCJsonPrintingDensity>? densities)
	{
		var printer = new XCJsonPrinter(ValidateDensities(root, densities));
		printer.Emit(root, XCJsonPath.Root);
		if (options.AddTrailingNewline)
			printer.Append("\n"u8);
		return printer.m_output.WrittenSpan.ToArray();
	}

	// matches Swift's Double.description, without a trailing ".0" (as Swift's JSONEncoder does)
	internal static string FormatDouble(double value)
	{
		if (!double.IsFinite(value))
			throw new XcodeProjectFormatException($"Unable to encode {value.ToString(CultureInfo.InvariantCulture)} as JSON");
		if (value == 0)
			return double.IsNegative(value) ? "-0" : "0";

		var roundTrip = Math.Abs(value).ToString("R", CultureInfo.InvariantCulture);

		ParseDigits(roundTrip, out var digits, out var exponent);

		var builder = new StringBuilder();
		if (value < 0)
			builder.Append('-');

		if (exponent < -4 || exponent >= 16)
		{
			builder.Append(digits[0]);
			if (digits.Length > 1)
				builder.Append('.').Append(digits, 1, digits.Length - 1);
			builder.Append('e').Append(exponent < 0 ? '-' : '+');
			builder.Append(Math.Abs(exponent).ToString("00", CultureInfo.InvariantCulture));
		}
		else if (exponent < 0)
		{
			builder.Append("0.").Append('0', -exponent - 1).Append(digits);
		}
		else if (digits.Length <= exponent + 1)
		{
			builder.Append(digits).Append('0', exponent + 1 - digits.Length);
		}
		else
		{
			builder.Append(digits, 0, exponent + 1).Append('.').Append(digits, exponent + 1, digits.Length - exponent - 1);
		}

		return builder.ToString();
	}

	private static void ParseDigits(string text, out string digits, out int exponent)
	{
		var mantissa = text;
		exponent = 0;
		var exponentIndex = text.IndexOfAny(['E', 'e']);
		if (exponentIndex >= 0)
		{
			mantissa = text[..exponentIndex];
			exponent = int.Parse(text[(exponentIndex + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
		}

		var pointIndex = mantissa.IndexOf('.', StringComparison.Ordinal);
		var integerPart = pointIndex >= 0 ? mantissa[..pointIndex] : mantissa;
		var fractionPart = pointIndex >= 0 ? mantissa[(pointIndex + 1)..] : "";
		var allDigits = integerPart + fractionPart;
		exponent += integerPart.Length - 1;

		var leadingZeros = 0;
		while (leadingZeros < allDigits.Length - 1 && allDigits[leadingZeros] == '0')
			leadingZeros++;
		exponent -= leadingZeros;
		digits = allDigits[leadingZeros..].TrimEnd('0');
		if (digits.Length == 0)
			digits = "0";
	}

	private XCJsonPrinter(Dictionary<XCJsonPath, XCJsonPrintingDensity> densities)
	{
		m_output = new ArrayBufferWriter<byte>();
		m_densities = densities;
		m_currentPath = XCJsonPath.Root;
	}

	private void Append(ReadOnlySpan<byte> utf8) => m_output.Write(utf8);

	private void Append(ReadOnlySpan<char> text)
	{
		var destination = m_output.GetSpan(Encoding.UTF8.GetMaxByteCount(text.Length));
		if (Utf8.FromUtf16(text, destination, out _, out var bytesWritten, replaceInvalidSequences: false) != OperationStatus.Done)
			throw new XcodeProjectFormatException($"Unable to encode invalid UTF-16 as UTF-8: {text.ToString().SmartQuoted()}");
		m_output.Advance(bytesWritten);
	}

	// matches the output of Swift's JSONEncoder with .withoutEscapingSlashes
	private void EmitString(string value)
	{
		Append("\""u8);
		var unescapedStart = 0;
		for (var index = 0; index < value.Length; index++)
		{
			var ch = value[index];
			if (ch is not ('"' or '\\') && ch >= ' ')
				continue;

			Append(value.AsSpan(unescapedStart, index - unescapedStart));
			unescapedStart = index + 1;
			switch (ch)
			{
				case '"':
					Append("\\\""u8);
					break;
				case '\\':
					Append("\\\\"u8);
					break;
				case '\n':
					Append("\\n"u8);
					break;
				case '\r':
					Append("\\r"u8);
					break;
				case '\t':
					Append("\\t"u8);
					break;
				case '\b':
					Append("\\b"u8);
					break;
				case '\f':
					Append("\\f"u8);
					break;
				default:
					Append("\\u00"u8);
					Append(((int) ch).ToString("x2", CultureInfo.InvariantCulture));
					break;
			}
		}
		Append(value.AsSpan(unescapedStart));
		Append("\""u8);
	}

	private bool IsCompact(XCJsonPath path) => m_densities.TryGetValue(path, out var density) && density == XCJsonPrintingDensity.Compact;

	private void Emit(XCJsonValue value, XCJsonPath path)
	{
		var originalPath = m_currentPath;
		var originalPrintOnSingleLine = m_printOnSingleLine;
		m_currentPath = path;
		m_printOnSingleLine = m_printOnSingleLine || IsCompact(path);

		switch (value.Type)
		{
			case XCJsonValueType.Null:
				Append("null"u8);
				break;
			case XCJsonValueType.Boolean:
				Append(value.BooleanValue ? "true"u8 : "false"u8);
				break;
			case XCJsonValueType.Integer:
				Append(value.IntegerValue.ToString(CultureInfo.InvariantCulture));
				break;
			case XCJsonValueType.Double:
				Append(FormatDouble(value.DoubleValue));
				break;
			case XCJsonValueType.String:
				EmitString(value.StringValue);
				break;
			case XCJsonValueType.Array:
				EmitArray(value.ArrayEntries);
				break;
			case XCJsonValueType.Object:
				EmitObject(value.ObjectEntries);
				break;
		}

		m_currentPath = originalPath;
		m_printOnSingleLine = originalPrintOnSingleLine;
	}

	private void EmitComment(XCJsonComment comment)
	{
		var content = comment.Content;
		switch (comment.Style)
		{
			case XCJsonCommentStyle.Line:
				if (m_printOnSingleLine)
					throw new InvalidOperationException("Line comments cannot be printed compactly.");
				Append("// "u8);
				for (var start = 0; ;)
				{
					var separatorIndex = IndexOfLineSeparator(content, start);
					Append(content.AsSpan(start, (separatorIndex < 0 ? content.Length : separatorIndex) - start));
					if (separatorIndex < 0)
						break;
					Append("\n"u8);
					IndentIfAtLineStart();
					Append("// "u8);
					start = separatorIndex + 1;
				}
				IndentIfAtLineStart();
				break;

			case XCJsonCommentStyle.Block:
				var outdented = content.StartsWith('\n');
				Append("/*"u8);
				if (!outdented)
					Append(" "u8);
				for (var start = 0; ;)
				{
					var separatorIndex = IndexOfLineSeparator(content, start);
					Append(content.AsSpan(start, (separatorIndex < 0 ? content.Length : separatorIndex) - start));
					if (separatorIndex < 0)
						break;
					if (m_printOnSingleLine)
						throw new InvalidOperationException("Multi-line comments cannot be printed compactly.");
					Append("\n"u8);
					IndentIfAtLineStart();
					if (!outdented)
						Append("   "u8);
					start = separatorIndex + 1;
				}
				if (!outdented)
					Append(" "u8);
				Append("*/"u8);
				break;
		}
	}

	// like Swift (which compares grapheme clusters), "\r\n" is not a line separator
	private static int IndexOfLineSeparator(string content, int startIndex)
	{
		for (var index = startIndex; index < content.Length; index++)
		{
			var ch = content[index];
			if (ch == '\r' && index + 1 < content.Length && content[index + 1] == '\n')
				index++;
			else if (XCJsonComment.IsJsonLineSeparator(ch))
				return index;
		}
		return -1;
	}

	private void IndentIfAtLineStart()
	{
		if (m_output.WrittenCount == 0 || m_output.WrittenSpan[^1] == (byte) '\n')
		{
			for (var i = 0; i < m_depth; i++)
				Append("  "u8);
		}
	}

	private void EmitArray(IReadOnlyList<XCJsonValueOrComment> entries)
	{
		var valueCount = entries.Count(x => x.IsValue);
		var valueIndex = 0;
		if (m_printOnSingleLine && entries.Count == 0)
		{
			Append("[]"u8);
		}
		else if (m_printOnSingleLine)
		{
			Append("[ "u8);
			foreach (var entry in entries)
			{
				if (entry.Value is { } value)
				{
					Emit(value, m_currentPath.Append(valueIndex));
					valueIndex++;
					if (valueIndex != valueCount)
						Append(", "u8);
				}
				else
				{
					EmitComment(entry.Comment!);
				}
			}
			Append(" ]"u8);
		}
		else
		{
			Append("[\n"u8);
			m_depth++;
			for (var entryIndex = 0; entryIndex < entries.Count; entryIndex++)
			{
				IndentIfAtLineStart();
				var entry = entries[entryIndex];
				if (entry.Value is { } value)
				{
					Emit(value, m_currentPath.Append(valueIndex));
					valueIndex++;
					var previousIsContainer = value.IsContainer;
					var previousIsMultiline = !IsCompact(m_currentPath.Append(valueIndex - 1));
					var nextIsContainer = entryIndex < entries.Count - 1 && entries[entryIndex + 1].Value is { IsContainer: true };
					var nextIsMultiline = !IsCompact(m_currentPath.Append(valueIndex));
					Append(previousIsContainer && previousIsMultiline && nextIsContainer && nextIsMultiline ? ", "u8 : ",\n"u8);
				}
				else
				{
					EmitComment(entry.Comment!);
					Append("\n"u8);
				}
			}
			m_depth--;
			IndentIfAtLineStart();
			Append("]"u8);
		}
	}

	private void EmitObject(IReadOnlyList<XCJsonFieldOrComment> entries)
	{
		var lastFieldIndex = -1;
		for (var index = 0; index < entries.Count; index++)
		{
			if (entries[index].IsField)
				lastFieldIndex = index;
		}

		if (m_printOnSingleLine && entries.Count == 0)
		{
			Append("{}"u8);
		}
		else if (m_printOnSingleLine)
		{
			Append("{ "u8);
			for (var index = 0; index < entries.Count; index++)
			{
				var entry = entries[index];
				if (entry.Field is { } field)
				{
					EmitField(field);
					if (index != lastFieldIndex)
						Append(", "u8);
				}
				else
				{
					EmitComment(entry.Comment!);
				}
			}
			Append(" }"u8);
		}
		else
		{
			Append("{\n"u8);
			m_depth++;
			foreach (var entry in entries)
			{
				IndentIfAtLineStart();
				if (entry.Field is { } field)
				{
					EmitField(field);
					Append(",\n"u8);
				}
				else
				{
					EmitComment(entry.Comment!);
					Append("\n"u8);
				}
			}
			m_depth--;
			IndentIfAtLineStart();
			Append("}"u8);
		}
	}

	private void EmitField(XCJsonField field)
	{
		EmitString(field.Key);
		Append(": "u8);
		Emit(field.Value, m_currentPath.Append(field.Key));
	}

	// removes requests for compact printing of values that contain content that can't be printed compactly
	private static Dictionary<XCJsonPath, XCJsonPrintingDensity> ValidateDensities(XCJsonValue root, IReadOnlyDictionary<XCJsonPath, XCJsonPrintingDensity>? densities)
	{
		var validated = densities is null ? new Dictionary<XCJsonPath, XCJsonPrintingDensity>() : new Dictionary<XCJsonPath, XCJsonPrintingDensity>(densities);
		if (validated.Count != 0)
			Validate(root, validated, XCJsonPath.Root);
		return validated;
	}

	private static bool Validate(XCJsonValue value, Dictionary<XCJsonPath, XCJsonPrintingDensity> densities, XCJsonPath path)
	{
		var probingLocally = densities.TryGetValue(path, out var density) && density == XCJsonPrintingDensity.Compact;
		var allowCompactness = true;

		switch (value.Type)
		{
			case XCJsonValueType.Array:
				var entries = value.ArrayEntries;
				for (var index = 0; index < entries.Count; index++)
				{
					if (entries[index].Value is { } child)
						allowCompactness = Validate(child, densities, path.Append(index)) && allowCompactness;
					else
						allowCompactness = entries[index].Comment!.AllowsCompactPrinting;
				}
				break;

			case XCJsonValueType.Object:
				foreach (var entry in value.ObjectEntries)
				{
					if (entry.Field is { } field)
						allowCompactness = Validate(field.Value, densities, path.Append(field.Key)) && allowCompactness;
					else
						allowCompactness = entry.Comment!.AllowsCompactPrinting;
				}
				break;
		}

		if (probingLocally && !allowCompactness)
			densities.Remove(path);
		return allowCompactness;
	}

	private readonly ArrayBufferWriter<byte> m_output;
	private readonly Dictionary<XCJsonPath, XCJsonPrintingDensity> m_densities;
	private int m_depth;
	private XCJsonPath m_currentPath;
	private bool m_printOnSingleLine;
}
