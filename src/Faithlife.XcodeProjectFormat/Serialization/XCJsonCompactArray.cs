using System.Collections;

namespace Faithlife.XcodeProjectFormat.Serialization;

/// <summary>
/// Wraps a sequence so that each element is printed compactly.
/// </summary>
internal sealed class XCJsonCompactArray(IEnumerable elements) : IXCJsonEncodable
{
	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenOrdinalContainer();
		foreach (var element in XCJsonCoding.GetOrderedElements(elements))
			container.Encode(element, XCJsonPrintingDensity.Compact);
	}
}
