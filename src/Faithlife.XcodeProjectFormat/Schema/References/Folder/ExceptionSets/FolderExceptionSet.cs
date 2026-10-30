using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// Whether the members of an exception set are included in, or excluded from, the target or build phase.
/// </summary>
public enum ExceptionSetSense
{
	Inclusions,
	Exclusions,
}

/// <summary>
/// Exceptions to the membership of a folder's contents in a target or build phase.
/// </summary>
public abstract class FolderExceptionSet : IXCJsonEncodable, IXCJsonDecodable<FolderExceptionSet>
{
	/// <summary>
	/// The sense of <see cref="MembershipExceptions"/>; this exists only for legibility of the file.
	/// </summary>
	public ExceptionSetSense Sense { get; set; }

	public HashSet<FolderMemberId> MembershipExceptions { get; set; } = [];

	public Dictionary<FolderMemberId, HashSet<PlatformFilter>> PlatformFiltersByFolderMemberId { get; set; } = [];

	public Dictionary<FolderMemberId, BuildFileAttributes> AttributesByFolderMemberId { get; set; } = [];

	public Dictionary<FolderMemberId, HashSet<AssetTag>> AssetTagsByFolderMemberId { get; set; } = [];

	private protected FolderExceptionSet()
	{
	}

	private protected abstract void EncodeFields(XCJsonEncoder.KeyedContainer container);

	private protected void EncodeCommonFields(XCJsonEncoder.KeyedContainer container)
	{
		if (MembershipExceptions.Count != 0)
			container.EncodeUnverified(GetKey(Sense), MembershipExceptions);
		if (PlatformFiltersByFolderMemberId.Count != 0)
			container.Encode("platforms", PlatformFiltersByFolderMemberId.WithCompactValueEncoding());
		if (AttributesByFolderMemberId.Count != 0)
			container.Encode("attributes", AttributesByFolderMemberId.WithCompactValueEncoding());
		if (AssetTagsByFolderMemberId.Count != 0)
			container.Encode("asset-tags", AssetTagsByFolderMemberId.WithCompactValueEncoding());
	}

	private protected void DecodeCommonFields(XCJsonDecoder.KeyedContainer container)
	{
		var senses = s_allSenses.Where(x => container.Contains(GetKey(x))).ToList();
		if (senses.Count == 1)
		{
			Sense = senses[0];
			MembershipExceptions = container.Decode(GetKey(Sense), new HashSet<FolderMemberId>());
		}
		else if (senses.Count == 0)
		{
			Sense = ExceptionSetSense.Inclusions;
			MembershipExceptions = [];
		}
		else
		{
			throw new XcodeProjectFormatException("Multiple exception set senses", $"Expected one of {GetKey(ExceptionSetSense.Inclusions).SmartQuoted()} or {GetKey(ExceptionSetSense.Exclusions).SmartQuoted()}");
		}

		PlatformFiltersByFolderMemberId = container.Decode("platforms", new Dictionary<FolderMemberId, HashSet<PlatformFilter>>());
		AttributesByFolderMemberId = container.Decode("attributes", new Dictionary<FolderMemberId, BuildFileAttributes>());
		AssetTagsByFolderMemberId = container.Decode("asset-tags", new Dictionary<FolderMemberId, HashSet<AssetTag>>());
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => EncodeFields(encoder.OpenKeyedContainer());

	static FolderExceptionSet IXCJsonDecodable<FolderExceptionSet>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		if (container.Contains("target"))
			return TargetExceptionSet.DecodeFields(container);
		if (container.Contains("build-phase"))
			return BuildPhaseExceptionSet.DecodeFields(container);
		throw new XcodeProjectFormatException("Unknown exception set. Expected either a target, or a build phase");
	}

	private static string GetKey(ExceptionSetSense sense) => sense == ExceptionSetSense.Exclusions ? "exclusions" : "inclusions";

	private static readonly ExceptionSetSense[] s_allSenses = [ExceptionSetSense.Inclusions, ExceptionSetSense.Exclusions];
}
