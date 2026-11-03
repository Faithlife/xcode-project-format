using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// Exceptions to the membership of a folder's contents in a target.
/// </summary>
public sealed class TargetExceptionSet : FolderExceptionSet
{
	public TargetExceptionSet(LocalTargetReference target) => Target = target ?? throw new ArgumentNullException(nameof(target));

	public LocalTargetReference Target { get; set; }

	public HashSet<FolderMemberId> PublicHeaders { get; set; } = [];

	public HashSet<FolderMemberId> PrivateHeaders { get; set; } = [];

	public Dictionary<FolderMemberId, string> AdditionalCompilerFlags { get; set; } = [];

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.Encode("target", Target);
		container.EncodeIfNotEmpty("public-headers", PublicHeaders);
		container.EncodeIfNotEmpty("private-headers", PrivateHeaders);
		container.EncodeIfNotEmpty("compiler-flags", AdditionalCompilerFlags);
		EncodeCommonFields(container);
	}

	internal static TargetExceptionSet DecodeFields(XCJsonDecoder.KeyedContainer container)
	{
		var exceptionSet = new TargetExceptionSet(container.Decode<LocalTargetReference>("target"))
		{
			PublicHeaders = container.Decode("public-headers", new HashSet<FolderMemberId>()),
			PrivateHeaders = container.Decode("private-headers", new HashSet<FolderMemberId>()),
			AdditionalCompilerFlags = container.Decode("compiler-flags", new Dictionary<FolderMemberId, string>()),
		};
		exceptionSet.DecodeCommonFields(container);
		return exceptionSet;
	}
}
