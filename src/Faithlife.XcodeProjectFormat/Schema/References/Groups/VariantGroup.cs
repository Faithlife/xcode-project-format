using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A group of localized variants of a file.
/// </summary>
public sealed class VariantGroup : Reference
{
	public VariantGroup(string name, FilePath? path = null)
		: base(path ?? new FilePath(FilePathBase.Group, ""))
	{
		Name = name ?? throw new ArgumentNullException(nameof(name));
	}

	public string Name { get; set; }

	public List<ProjectBuildFile> BuildFiles { get; set; } = [];

	public List<FileReference> Children { get; set; } = [];

	private protected override ReferenceKind Kind => ReferenceKind.VariantGroup;

	private protected override XCJsonPrintingDensity? PrintingDensity => Children.Count == 0 && BuildFiles.Count <= 1 ? XCJsonPrintingDensity.Compact : null;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeInline(Path);
		container.EncodeIfNotDefault("name", Name, Path.LastPathComponent);
		EncodeIncludeInIndex(container);
		container.EncodeIfNotEmpty("target-membership", BuildFiles);
		container.EncodeIfNotEmpty("children", Children.Select(x => new FileReference.Child(x)).ToList());
	}

	internal static VariantGroup DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var objectId = container.DecodeOptional<ObjectId>("id");
		var path = container.DecodeInline<FilePath>();
		return new VariantGroup(container.Decode("name", path.LastPathComponent), path)
		{
			ObjectId = objectId,
			IncludeInIndex = container.DecodeOptionalValue<bool>("index"),
			BuildFiles = container.Decode("target-membership", new List<ProjectBuildFile>()),
			Children = container.Decode("children", new List<FileReference.Child>()).Select(x => x.Reference).ToList(),
		};
	}
}
