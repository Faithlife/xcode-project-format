using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to a file.
/// </summary>
public sealed class FileReference : Reference
{
	public FileReference(FilePath path)
		: base(path)
	{
	}

	public FileTypeId? ExplicitFileType { get; set; }

	public string? ExpectedSignature { get; set; }

	public TextEncoding? TextEncoding { get; set; }

	public LineEnding? LineEnding { get; set; }

	/// <summary>
	/// The build phases that this file is a member of.
	/// </summary>
	public List<ProjectBuildFile> BuildFiles { get; set; } = [];

	private protected override ReferenceKind Kind => ReferenceKind.FileReference;

	private protected override XCJsonPrintingDensity? PrintingDensity => BuildFiles.Count <= 1 ? XCJsonPrintingDensity.Compact : null;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeInline(Path);
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeIfNotNull("type", ExplicitFileType);
		container.EncodeIfNotNull("signature", ExpectedSignature);
		container.EncodeIfNotNull("encoding", TextEncoding);
		container.EncodeIfNotNull("line-ending", LineEnding);
		EncodeIncludeInIndex(container);
		container.EncodeIfNotEmpty("target-membership", BuildFiles);
	}

	internal static FileReference DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var objectId = container.DecodeOptional<ObjectId>("id");
		var reference = new FileReference(container.DecodeInline<FilePath>())
		{
			ObjectId = objectId,
			ExplicitFileType = container.DecodeOptional<FileTypeId>("type"),
			ExpectedSignature = container.DecodeOptional<string>("signature"),
			TextEncoding = container.DecodeOptionalValue<TextEncoding>("encoding"),
			LineEnding = container.DecodeOptionalValue<LineEnding>("line-ending"),
			IncludeInIndex = container.DecodeOptionalValue<bool>("index"),
			BuildFiles = container.Decode("target-membership", new List<ProjectBuildFile>()),
		};
		return reference;
	}

	// Encodes a file reference that is a child of a variant or version group (without the "kind" field).
	internal sealed class Child(FileReference reference) : IXCJsonEncodable, IXCJsonDecodable<Child>
	{
		public FileReference Reference { get; } = reference;

		void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => Reference.EncodeFields(encoder.OpenKeyedContainer());

		static Child IXCJsonDecodable<Child>.Decode(XCJsonDecoder decoder) => new(DecodeFields(decoder.OpenKeyedContainer()));
	}
}
