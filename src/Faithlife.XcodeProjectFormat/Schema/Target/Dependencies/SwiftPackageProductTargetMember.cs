using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The membership of a Swift package product in a build phase of a target.
/// </summary>
public sealed class SwiftPackageProductTargetMember : IXCJsonEncodable, IXCJsonDecodable<SwiftPackageProductTargetMember>
{
	public SwiftPackageProductTargetMember(SwiftPackageProductReference packageProduct, TargetBuildFile buildFile)
	{
		PackageProduct = packageProduct ?? throw new ArgumentNullException(nameof(packageProduct));
		BuildFile = buildFile ?? throw new ArgumentNullException(nameof(buildFile));
	}

	public SwiftPackageProductReference PackageProduct { get; set; }

	public TargetBuildFile BuildFile { get; set; }

	internal static int CompareForEncoding(SwiftPackageProductTargetMember left, SwiftPackageProductTargetMember right)
	{
		var leftKey = left.GetEncodingOrder();
		var rightKey = right.GetEncodingOrder();
		for (var index = 0; index < leftKey.Length; index++)
		{
			var comparison = XCJsonCoding.CompareStrings(leftKey[index], rightKey[index]);
			if (comparison != 0)
				return comparison;
		}
		return 0;
	}

	private string[] GetEncodingOrder()
	{
		var phase = BuildFile.BuildPhase;
		return
		[
			PackageProduct.Package?.PackageName ?? "",
			PackageProduct.ProductName,
			XCJsonCoding.GetEnumName(PackageProduct.ProductType),
			phase.Kind is { } kind ? XCJsonCoding.GetEnumName(kind) : "",
			phase.Kind is not null ? phase.Name ?? "" : "",
			phase.ObjectId?.Value ?? "",
		];
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		PackageProduct.EncodeFields(container);
		container.Encode("build-phase", BuildFile);
	}

	static SwiftPackageProductTargetMember IXCJsonDecodable<SwiftPackageProductTargetMember>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		var product = SwiftPackageProductReference.DecodeFields(container);
		return new SwiftPackageProductTargetMember(product, container.Decode<TargetBuildFile>("build-phase"));
	}
}
