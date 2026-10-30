using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A Swift package that the project depends on.
/// </summary>
public sealed class SwiftPackage : IXCJsonEncodable, IXCJsonDecodable<SwiftPackage>
{
	public SwiftPackage(SwiftPackageLocation location) => Location = location ?? throw new ArgumentNullException(nameof(location));

	public SwiftPackageLocation Location { get; set; }

	public List<string> Traits { get; set; } = [];

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		Location.EncodeFields(container);
		container.EncodeIfNotEmpty("traits", Traits);
	}

	static SwiftPackage IXCJsonDecodable<SwiftPackage>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		return new SwiftPackage(SwiftPackageLocation.DecodeFields(container))
		{
			Traits = container.Decode("traits", new List<string>()),
		};
	}
}
