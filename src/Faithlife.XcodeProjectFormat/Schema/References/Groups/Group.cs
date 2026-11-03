using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A group of references.
/// </summary>
public sealed class Group : Reference
{
	public Group(FilePath path, string? name = null)
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

	public List<Reference> Children { get; set; } = [];

	private protected override ReferenceKind Kind => ReferenceKind.Group;

	private protected override XCJsonPrintingDensity? PrintingDensity => Children.Count == 0 ? XCJsonPrintingDensity.Compact : null;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeInline(Path);
		container.EncodeIfNotDefault("name", Name, Path.LastPathComponent);
		EncodeIncludeInIndex(container);
		container.EncodeIfNotEmpty("children", Children);
	}

	internal static Group DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var objectId = container.DecodeOptional<ObjectId>("id");
		var group = new Group(container.DecodeInline<FilePath>(), container.DecodeOptional<string>("name")) { ObjectId = objectId };
		group.IncludeInIndex = container.DecodeOptionalValue<bool>("index");
		group.Children = container.Decode("children", new List<Reference>());
		return group;
	}

	private string? m_name;
}
