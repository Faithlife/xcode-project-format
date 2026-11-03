using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A reference to a build phase of a target, from a file in the project's group tree.
/// </summary>
/// <remarks>A named reference is encoded as a name path, for example, <c>MyApp/compile-sources</c> or <c>MyApp/copy/Copy Plug-Ins</c>.</remarks>
public sealed class ProjectBuildPhaseReference : IEquatable<ProjectBuildPhaseReference>, IXCJsonEncodable, IXCJsonDecodable<ProjectBuildPhaseReference>
{
	/// <summary>
	/// Creates a reference to the build phase of the specified target with the specified kind and (optional) name.
	/// </summary>
	public ProjectBuildPhaseReference(LocalTargetReference target, BuildPhaseKind kind, string? name = null)
	{
		Target = target ?? throw new ArgumentNullException(nameof(target));
		Kind = kind;
		Name = name;
	}

	/// <summary>
	/// Creates a reference to the build phase with the specified identifier.
	/// </summary>
	public ProjectBuildPhaseReference(ObjectId objectId) => ObjectId = objectId ?? throw new ArgumentNullException(nameof(objectId));

	/// <summary>
	/// Creates a reference from a slash-separated name path, for example, <c>MyApp/compile-sources</c>.
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The value isn't a valid reference.</exception>
	public ProjectBuildPhaseReference(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		(Target, Kind, Name) = ParseNamePath(new NamePath(value.Split('/').Select(NamePathComponent.Child)));
	}

	public static ProjectBuildPhaseReference FromString(string value) => new(value);

	public static implicit operator ProjectBuildPhaseReference(string value) => FromString(value);

	public ObjectId? ObjectId { get; }

	public LocalTargetReference? Target { get; }

	public BuildPhaseKind? Kind { get; }

	public string? Name { get; }

	public bool Equals(ProjectBuildPhaseReference? other) =>
		other is not null && Equals(ObjectId, other.ObjectId) && Equals(Target, other.Target) && Kind == other.Kind && Name == other.Name;

	public override bool Equals(object? obj) => obj is ProjectBuildPhaseReference other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(ObjectId, Target, Kind, Name);

	public override string ToString() => GroupTreeReferenceRepresentation.ToString();

	internal GroupTreeReference GroupTreeReferenceRepresentation => ObjectId is not null ? new GroupTreeReference(ObjectId) : new GroupTreeReference(NamePath.FromChildNames(new[] { Target!.TargetName, XCJsonCoding.GetEnumName(Kind!.Value), Name }.OfType<string>()));

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder) => ((IXCJsonEncodable) GroupTreeReferenceRepresentation).Encode(encoder);

	static ProjectBuildPhaseReference IXCJsonDecodable<ProjectBuildPhaseReference>.Decode(XCJsonDecoder decoder)
	{
		var reference = decoder.Decode<GroupTreeReference>();
		if (reference.ObjectId is not null)
			return new ProjectBuildPhaseReference(reference.ObjectId);
		var (target, kind, name) = ParseNamePath(reference.NamePath!);
		return new ProjectBuildPhaseReference(target, kind, name);
	}

	private static (LocalTargetReference Target, BuildPhaseKind Kind, string? Name) ParseNamePath(NamePath path)
	{
		if (path.Components.Count == 0 || path.Components[0].ChildName is not { } targetName)
			throw new XcodeProjectFormatException($"Invalid project relative build phase reference {path.ToString().SmartQuoted()}");
		var (kind, name) = TargetBuildPhaseReference.ParseNamePath(path.Components.Skip(1).ToList());
		return (new LocalTargetReference(targetName), kind, name);
	}
}
