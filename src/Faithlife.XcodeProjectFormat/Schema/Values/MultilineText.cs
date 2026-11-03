using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// Text that is encoded as an array of lines when it has more than one line.
/// </summary>
internal sealed record MultilineText(string Text) : IXCJsonEncodable, IXCJsonDecodable<MultilineText>
{
	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var lines = Text.Split('\n');
		if (lines.Length == 1 || (lines.Length == 2 && lines[1].Length == 0))
		{
			encoder.OpenPrimitiveContainer().Encode(Text);
		}
		else
		{
			var container = encoder.OpenOrdinalContainer();
			foreach (var line in lines)
				container.Encode(line);
		}
	}

	static MultilineText IXCJsonDecodable<MultilineText>.Decode(XCJsonDecoder decoder) =>
		new(decoder.CurrentNodeType == XCJsonValueType.String ? decoder.DecodePrimitiveString() : string.Join("\n", decoder.Decode<List<string>>()));
}
