using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to a product of a Swift package.
/// </summary>
public sealed record SwiftPackageProductReference : IXCJsonEncodable, IXCJsonDecodable<SwiftPackageProductReference>
{
	public SwiftPackageProductReference(string productName, SwiftPackageName? package = null, SwiftPackageProductType productType = SwiftPackageProductType.Other, ObjectId? objectId = null)
	{
		ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
		Package = package;
		ProductType = productType;
		ObjectId = objectId;
	}

	public ObjectId? ObjectId { get; init; }

	public SwiftPackageName? Package { get; init; }

	public string ProductName { get; init; }

	public SwiftPackageProductType ProductType { get; init; }

	internal void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("package", Package);
		container.EncodeIfNotNull("id", ObjectId);
		container.Encode("product-name", ProductName);
		container.EncodeIfNotDefault("product-type", ProductType, SwiftPackageProductType.Other);
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => EncodeFields(encoder.OpenKeyedContainer());

	static SwiftPackageProductReference IXCJsonDecodable<SwiftPackageProductReference>.Decode(XCJsonDecoder decoder) => DecodeFields(decoder.OpenKeyedContainer());

	internal static SwiftPackageProductReference DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var package = container.DecodeOptional<SwiftPackageName>("package");
		var objectId = container.DecodeOptional<ObjectId>("id");
		return new SwiftPackageProductReference(container.Decode<string>("product-name"), package, container.Decode("product-type", SwiftPackageProductType.Other), objectId);
	}
}
