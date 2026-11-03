using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to a build phase of a target, from a file in a folder reference or a package product.
/// </summary>
/// <remarks>A named reference is encoded as a name path, for example, <c>compile-sources</c> or <c>copy/Copy Plug-Ins</c>.</remarks>
public sealed class TargetBuildPhaseReference : IEquatable<TargetBuildPhaseReference>, IXCJsonEncodable, IXCJsonDecodable<TargetBuildPhaseReference>
{
	/// <summary>
	/// Creates a reference to the build phase with the specified kind and (optional) name.
	/// </summary>
	public TargetBuildPhaseReference(BuildPhaseKind kind, string? name = null)
	{
		Kind = kind;
		Name = name;
	}

	/// <summary>
	/// Creates a reference to the build phase with the specified identifier.
	/// </summary>
	public TargetBuildPhaseReference(ObjectId objectId) => ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));

	/// <summary>
	/// Creates a reference from a slash-separated name path, for example, <c>compile-sources</c>.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The value isn't a valid reference.</exception>
	public TargetBuildPhaseReference(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		(Kind, Name) = ParseNamePath(value.Split('/').Select(NamePathComponent.Child).ToList());
	}

	public static TargetBuildPhaseReference FromString(string value) => new(value);

	public static implicit operator TargetBuildPhaseReference(string value) => FromString(value);

	public ObjectId? ObjectId { get; }

	public BuildPhaseKind? Kind { get; }

	public string? Name { get; }

	public bool Equals(TargetBuildPhaseReference? other) => other is not null && Equals(ObjectId, other.ObjectId) && Kind == other.Kind && Name == other.Name;

	public override bool Equals(object? obj) => obj is TargetBuildPhaseReference other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(ObjectId, Kind, Name);

	public override string ToString() => GroupTreeReferenceRepresentation.ToString();

	internal GroupTreeReference GroupTreeReferenceRepresentation => ObjectId is not null ? new GroupTreeReference(ObjectId) : new GroupTreeReference(NamePath.FromChildNames(new[] { XCJsonCoding.GetEnumName(Kind!.Value), Name }.OfType<string>()));

	internal static (BuildPhaseKind Kind, string? Name) ParseNamePath(IReadOnlyList<NamePathComponent> components)
	{
		if (components.Count is >= 1 and <= 2 &&
			components[0].ChildName is { } kindName &&
			XCJsonCoding.TryParseEnumName<BuildPhaseKind>(kindName, out var kind))
		{
			if (components.Count == 1)
				return (kind, null);
			if (components[1].ChildName is { } name)
				return (kind, name);
		}

		throw new XcodeProjectFormatException($"Invalid target relative build phase reference: {new NamePath(components).ToString().SmartQuoted()}");
	}

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => ((IXCJsonEncodable) GroupTreeReferenceRepresentation).Encode(encoder);

	static TargetBuildPhaseReference IXCJsonDecodable<TargetBuildPhaseReference>.Decode(XCJsonDecoder decoder)
	{
		var reference = decoder.Decode<GroupTreeReference>();
		if (reference.ObjectId is not null)
			return new TargetBuildPhaseReference(reference.ObjectId);
		var (kind, name) = ParseNamePath(reference.NamePath!.Components);
		return new TargetBuildPhaseReference(kind, name);
	}
}
