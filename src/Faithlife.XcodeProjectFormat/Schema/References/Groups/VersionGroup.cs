using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A group of versions of a file (for example, a Core Data model).
/// </summary>
public sealed class VersionGroup : Reference
{
	public VersionGroup(FilePath path, string? name = null)
		: base(path)
	{
		m_name = name;
	}

	/// <summary>
	/// The name of the group; defaults to the last path component of <see cref="Reference.Path"/>.
	/// </summary>
	public string Name
	{
		get => m_name ?? Path.LastPathComponent;
		set => m_name = value;
	}

	public GroupTreeReference? CurrentVersion { get; set; }

	public FileTypeId? VersionedFileType { get; set; }

	public List<ProjectBuildFile> BuildFiles { get; set; } = [];

	public List<FileReference> Children { get; set; } = [];

	private protected override ReferenceKind Kind => ReferenceKind.VersionGroup;

	private protected override XCJsonPrintingDensity? PrintingDensity => Children.Count == 0 && BuildFiles.Count <= 1 ? XCJsonPrintingDensity.Compact : null;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeInline(Path);
		container.EncodeIfNotDefault("name", Name, Path.LastPathComponent);
		container.EncodeIfNotNull("current-version", CurrentVersion);
		container.EncodeIfNotNull("type", VersionedFileType);
		EncodeIncludeInIndex(container);
		container.EncodeIfNotEmpty("target-membership", BuildFiles);
		container.EncodeIfNotEmpty("children", Children.Select(x => new FileReference.Child(x)).ToList());
	}

	internal static VersionGroup DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var objectId = container.DecodeOptional<ObjectId>("id");
		return new VersionGroup(container.DecodeInline<FilePath>(), container.DecodeOptional<string>("name"))
		{
			ObjectId = objectId,
			CurrentVersion = container.DecodeOptional<GroupTreeReference>("current-version"),
			VersionedFileType = container.DecodeOptional<FileTypeId>("type"),
			IncludeInIndex = container.DecodeOptionalValue<bool>("index"),
			BuildFiles = container.Decode("target-membership", new List<ProjectBuildFile>()),
			Children = container.Decode("children", new List<FileReference.Child>()).Select(x => x.Reference).ToList(),
		};
	}

	private string? m_name;
}
