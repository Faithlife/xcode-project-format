using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to an item in the group tree, relative to an anchor item.
/// </summary>
public sealed record GroupTreeAnchoredReference(GroupTreeReference Anchor, NamePath? RelativePath = null) : IXCJsonEncodable, IXCJsonDecodable<GroupTreeAnchoredReference>
{
	public GroupTreeReference Anchor { get; init; } = Anchor ?? throw new ArgumentNullException(nameof(Anchor));

	public override string ToString() => RelativePath is not null ? Anchor + "/" + RelativePath : Anchor.ToString();

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (RelativePath is not null)
		{
			var container = encoder.OpenKeyedContainer();
			container.Encode("anchor", Anchor);
			container.Encode("relative-path", RelativePath);
		}
		else
		{
			((IXCJsonEncodable) Anchor).Encode(encoder);
		}
	}

	static GroupTreeAnchoredReference IXCJsonDecodable<GroupTreeAnchoredReference>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType == XCJsonValueType.Object)
		{
			var container = decoder.OpenKeyedContainer();
			return new GroupTreeAnchoredReference(container.Decode<GroupTreeReference>("anchor"), container.Decode<NamePath>("relative-path"));
		}
		return new GroupTreeAnchoredReference(decoder.Decode<GroupTreeReference>());
	}
}
