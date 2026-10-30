using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A dependency on a Swift package product.
/// </summary>
public sealed class PackageProductDependency : TargetDependency
{
	public PackageProductDependency(SwiftPackageProductReference product) => Product = product ?? throw new ArgumentNullException(nameof(product));

	public SwiftPackageProductReference Product { get; set; }

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container) => Product.EncodeFields(container);
}
