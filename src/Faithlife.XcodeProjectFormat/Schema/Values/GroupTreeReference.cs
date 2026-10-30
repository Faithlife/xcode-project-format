using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to an item in the group tree, either by object identifier or by name path.
/// </summary>
public sealed class GroupTreeReference : IEquatable<GroupTreeReference>, IXCJsonEncodable, IXCJsonDecodable<GroupTreeReference>
{
	public GroupTreeReference(ObjectId objectId) => ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));

	public GroupTreeReference(NamePath namePath) => NamePath = namePath ?? throw new ArgumentNullException(nameof(namePath));

	public static GroupTreeReference FromObjectId(ObjectId objectId) => new(objectId);

	public static GroupTreeReference FromNamePath(NamePath namePath) => new(namePath);

	public static implicit operator GroupTreeReference(ObjectId objectId) => FromObjectId(objectId);

	public static implicit operator GroupTreeReference(NamePath namePath) => FromNamePath(namePath);

	/// <summary>
	/// The object identifier, if this references an item by identifier; otherwise null.
	/// </summary>
	public ObjectId? ObjectId { get; }

	/// <summary>
	/// The name path, if this references an item by name; otherwise null.
	/// </summary>
	public NamePath? NamePath { get; }

	public bool Equals(GroupTreeReference? other) => other is not null && Equals(ObjectId, other.ObjectId) && Equals(NamePath, other.NamePath);

	public override bool Equals(object? obj) => obj is GroupTreeReference other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(ObjectId, NamePath);

	public override string ToString() => ObjectId is not null ? c_idSignalingPrefix + ObjectId.Value : NamePath!.ToString();

	internal bool EncodesToString => EncodedString is not null;

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (EncodedString is { } text)
			encoder.OpenPrimitiveContainer().Encode(text);
		else
			NamePath.EncodeComponents(encoder, NamePath!.Components);
	}

	static GroupTreeReference IXCJsonDecodable<GroupTreeReference>.Decode(XCJsonDecoder decoder)
	{
		if (decoder.CurrentNodeType != XCJsonValueType.String)
			return new GroupTreeReference(new NamePath(decoder.Decode<List<NamePathComponent>>()));

		var text = decoder.DecodePrimitiveString();
		return text.StartsWith(c_idSignalingPrefix, StringComparison.Ordinal) ? new GroupTreeReference(new ObjectId(text[c_idSignalingPrefix.Length..])) : new GroupTreeReference(NamePath.ParseLosslessPathRepresentation(text));
	}

	private string? EncodedString
	{
		get
		{
			if (ObjectId is not null)
				return c_idSignalingPrefix + ObjectId.Value;
			var path = NamePath!.LosslessPathRepresentation;
			return path is not null && !path.StartsWith(c_idSignalingPrefix, StringComparison.Ordinal) ? path : null;
		}
	}

	private const string c_idSignalingPrefix = "id:";
}
