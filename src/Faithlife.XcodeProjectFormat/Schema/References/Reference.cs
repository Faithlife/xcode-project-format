using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// An item in the project's group tree.
/// </summary>
public abstract class Reference : IXCJsonEncodable, IXCJsonDecodable<Reference>
{
	public ObjectId? ObjectId { get; set; }

	public FilePath Path { get; set; }

	/// <summary>
	/// Whether the item should be indexed, or <c>null</c> to use the default behavior.
	/// </summary>
	public bool? IncludeInIndex { get; set; }

	private protected Reference(FilePath path) => Path = path ?? throw new ArgumentNullException(nameof(path));

	internal enum ReferenceKind
	{
		[XCJsonName("file-reference")]
		FileReference,

		[XCJsonName("group")]
		Group,

		[XCJsonName("folder")]
		Folder,

		[XCJsonName("variant-group")]
		VariantGroup,

		[XCJsonName("version-group")]
		VersionGroup,
	}

	private protected abstract ReferenceKind Kind { get; }

	private protected abstract XCJsonPrintingDensity? PrintingDensity { get; }

	private protected abstract void EncodeFields(XCJsonEncoder.KeyedContainer container);

	private protected void EncodeIncludeInIndex(XCJsonEncoder.KeyedContainer container) => container.EncodeIfNotNull("index", IncludeInIndex);

	private protected void DecodeCommonFields(XCJsonDecoder.KeyedContainer container)
	{
		ObjectId = container.DecodeOptional<ObjectId>("id");
		IncludeInIndex = container.DecodeOptionalValue<bool>("index");
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer(PrintingDensity);
		container.EncodeIfNotDefault("kind", Kind, ReferenceKind.FileReference);
		EncodeFields(container);
	}

	static Reference IXCJsonDecodable<Reference>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		return container.Decode("kind", ReferenceKind.FileReference) switch
		{
			ReferenceKind.FileReference => FileReference.DecodeFields(container),
			ReferenceKind.Group => Group.DecodeFields(container),
			ReferenceKind.Folder => Folder.DecodeFields(container),
			ReferenceKind.VariantGroup => VariantGroup.DecodeFields(container),
			ReferenceKind.VersionGroup => VersionGroup.DecodeFields(container),
			var kind => throw new InvalidOperationException($"Unexpected reference kind {kind}."),
		};
	}
}
