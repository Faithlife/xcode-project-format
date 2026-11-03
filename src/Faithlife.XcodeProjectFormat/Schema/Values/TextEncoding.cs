using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A text encoding, identified by its <c>NSStringEncoding</c> value.
/// </summary>
public readonly record struct TextEncoding(ulong Value) : IXCJsonEncodable, IXCJsonDecodable<TextEncoding>
{
	public static readonly TextEncoding Ascii = new(1);
	public static readonly TextEncoding NextStep = new(2);
	public static readonly TextEncoding JapaneseEuc = new(3);
	public static readonly TextEncoding Utf8 = new(4);
	public static readonly TextEncoding IsoLatin1 = new(5);
	public static readonly TextEncoding Symbol = new(6);
	public static readonly TextEncoding NonLossyAscii = new(7);
	public static readonly TextEncoding ShiftJis = new(8);
	public static readonly TextEncoding IsoLatin2 = new(9);
	public static readonly TextEncoding Unicode = new(10);
	public static readonly TextEncoding WindowsCodePage1251 = new(11);
	public static readonly TextEncoding WindowsCodePage1252 = new(12);
	public static readonly TextEncoding WindowsCodePage1253 = new(13);
	public static readonly TextEncoding WindowsCodePage1254 = new(14);
	public static readonly TextEncoding WindowsCodePage1250 = new(15);
	public static readonly TextEncoding Iso2022Jp = new(21);
	public static readonly TextEncoding MacOSRoman = new(30);
	public static readonly TextEncoding Utf16 = Unicode;
	public static readonly TextEncoding Utf16BigEndian = new(0x90000100);
	public static readonly TextEncoding Utf16LittleEndian = new(0x94000100);
	public static readonly TextEncoding Utf32 = new(0x8c000100);
	public static readonly TextEncoding Utf32BigEndian = new(0x98000100);
	public static readonly TextEncoding Utf32LittleEndian = new(0x9c000100);

	/// <summary>
	/// The name used to encode the text encoding, or null if it is encoded as a number.
	/// </summary>
	public string? Name
	{
		get
		{
			foreach (var (name, value) in s_names)
			{
				if (value == Value)
					return name;
			}
			return null;
		}
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (Name is { } name)
			encoder.OpenPrimitiveContainer().Encode(name);
		else
			encoder.OpenPrimitiveContainer().Encode(unchecked((long) Value));
	}

	static TextEncoding IXCJsonDecodable<TextEncoding>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.String)
		{
			var name = decoder.DecodePrimitiveString();
			foreach (var (candidateName, value) in s_names)
			{
				if (candidateName == name)
					return new TextEncoding(value);
			}
			throw new XcodeProjectFormatException($"Unexpected value {name.SmartQuoted()}");
		}

		var number = decoder.DecodePrimitiveInteger();
		if (number < 0)
			throw new XcodeProjectFormatException($"Invalid text encoding {number}");
		return new TextEncoding((ulong) number);
	}

	// the first name for a value is used when encoding
	private static readonly (string Name, ulong Value)[] s_names =
	[
		("ascii", 1),
		("nextstep", 2),
		("japanese-euc", 3),
		("utf8", 4),
		("iso-latin-1", 5),
		("symbol", 6),
		("non-lossy-ascii", 7),
		("shift-jis", 8),
		("iso-latin-2", 9),
		("unicode", 10),
		("windows-code-page-1251", 11),
		("windows-code-page-1252", 12),
		("windows-code-page-1253", 13),
		("windows-code-page-1254", 14),
		("windows-code-page-1250", 15),
		("iso-2022-jp", 21),
		("macos-roman", 30),
		("utf16", 10),
		("utf16-big-endian", 0x90000100),
		("utf16-little-endian", 0x94000100),
		("utf32", 0x8c000100),
		("utf32-big-endian", 0x98000100),
		("utf32-little-endian", 0x9c000100),
	];
}
