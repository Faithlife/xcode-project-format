using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A dependency of a target.
/// </summary>
public abstract class TargetDependency : IXCJsonEncodable, IXCJsonDecodable<TargetDependency>
{
	/// <summary>
	/// The platforms for which the dependency applies; empty for all platforms.
	/// </summary>
	public HashSet<PlatformFilter> PlatformFilters { get; set; } = [];

	private protected TargetDependency()
	{
	}

	private enum DependencyKind
	{
		[XCJsonName("localTarget")]
		LocalTarget,

		[XCJsonName("remoteTarget")]
		RemoteTarget,

		[XCJsonName("package")]
		Package,
	}

	private DependencyKind Kind => this switch
	{
		RemoteTargetDependency => DependencyKind.RemoteTarget,
		PackageProductDependency => DependencyKind.Package,
		_ => DependencyKind.LocalTarget,
	};

	private protected abstract void EncodeFields(XCJsonEncoder.KeyedContainer container);

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (this is LocalTargetDependency local && PlatformFilters.Count == 0)
		{
			encoder.OpenPrimitiveContainer().Encode(local.Target.TargetName);
		}
		else
		{
			var container = encoder.OpenKeyedContainer(XCJsonPrintingDensity.Compact);
			container.EncodeIfNotDefault("kind", Kind, DependencyKind.LocalTarget);
			EncodeFields(container);
			container.EncodeIfNotEmpty("platforms", PlatformFilters, XCJsonPrintingDensity.Compact);
		}
	}

	static TargetDependency IXCJsonDecodable<TargetDependency>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.String)
			return new LocalTargetDependency(new LocalTargetReference(decoder.DecodePrimitiveString()));

		var container = decoder.OpenKeyedContainer();
		var filters = container.Decode("platforms", new HashSet<PlatformFilter>());
		TargetDependency dependency = container.Decode("kind", DependencyKind.LocalTarget) switch
		{
			DependencyKind.RemoteTarget => new RemoteTargetDependency(
				container.Decode<GroupTreeReference>("project"),
				container.Decode<string>("target"),
				container.Decode<ObjectId>("target-id")),
			DependencyKind.Package => new PackageProductDependency(SwiftPackageProductReference.DecodeFields(container)),
			_ => new LocalTargetDependency(container.Decode<LocalTargetReference>("target")),
		};
		dependency.PlatformFilters = filters;
		return dependency;
	}
}
