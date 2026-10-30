using System.Collections;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Wraps a dictionary so that each value is printed compactly.
/// </summary>
internal sealed class XCJsonCompactDictionary(IDictionary dictionary) : IXCJsonEncodable
{
	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => XCJsonCoding.EncodeDictionary(encoder, dictionary, XCJsonPrintingDensity.Compact);
}
