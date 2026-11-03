using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A folder whose contents are synchronized with the file system.
/// </summary>
public sealed class Folder : Reference
{
	public Folder(FilePath path)
		: base(path)
	{
	}

	/// <summary>
	/// The targets that the folder's contents are members of.
	/// </summary>
	public HashSet<LocalTargetReference> Targets { get; set; } = [];

	public List<FolderExceptionSet> MembershipExceptions { get; set; } = [];

	public Dictionary<FolderMemberId, FileTypeId> ExplicitFileTypes { get; set; } = [];

	public HashSet<FolderMemberId> ExplicitOpaqueFolders { get; set; } = [];

	private protected override ReferenceKind Kind => ReferenceKind.Folder;

	private protected override XCJsonPrintingDensity? PrintingDensity => MembershipExceptions.Count == 0 ? XCJsonPrintingDensity.Compact : null;

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("id", ObjectId);
		container.EncodeInline(Path);
		container.EncodeIfNotEmpty("file-types", ExplicitFileTypes);
		container.EncodeIfNotEmpty("opaque-folders", ExplicitOpaqueFolders);
		container.EncodeIfNotEmpty("target-membership", Targets);
		container.EncodeIfNotEmpty("membership-exceptions", MembershipExceptions);
		EncodeIncludeInIndex(container);
	}

	internal static Folder DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var path = container.DecodeInline<FilePath>();
		return new Folder(path)
		{
			ObjectId = container.DecodeOptional<ObjectId>("id"),
			ExplicitFileTypes = container.Decode("file-types", new Dictionary<FolderMemberId, FileTypeId>()),
			ExplicitOpaqueFolders = container.Decode("opaque-folders", new HashSet<FolderMemberId>()),
			Targets = container.Decode("target-membership", new HashSet<LocalTargetReference>()),
			MembershipExceptions = container.Decode("membership-exceptions", new List<FolderExceptionSet>()),
			IncludeInIndex = container.DecodeOptionalValue<bool>("index"),
		};
	}
}
