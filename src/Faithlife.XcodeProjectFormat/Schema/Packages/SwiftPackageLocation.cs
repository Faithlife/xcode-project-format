using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The location of a Swift package.
/// </summary>
public abstract class SwiftPackageLocation : IXCJsonEncodable, IXCJsonDecodable<SwiftPackageLocation>
{
	private protected SwiftPackageLocation()
	{
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => EncodeFields(encoder.OpenKeyedContainer());

	static SwiftPackageLocation IXCJsonDecodable<SwiftPackageLocation>.Decode(XCJsonDecoder decoder) => DecodeFields(decoder.OpenKeyedContainer());

	internal abstract void EncodeFields(XCJsonEncoder.KeyedContainer container);

	internal static SwiftPackageLocation DecodeFields(XCJsonDecoder.KeyedContainer container) => container.Decode<string>("kind") switch
	{
		"local" => new LocalSwiftPackage(container.Decode<string>("path")),
		"remote" => new RemoteSwiftPackage(container.Decode<string>("repository"))
		{
			VersionConstraint = container.DecodeOptional<SwiftPackageVersionConstraint>("version"),
		},
		var kind => throw new XcodeProjectFormatException($"Unexpected value {kind.SmartQuoted()}"),
	};
}
