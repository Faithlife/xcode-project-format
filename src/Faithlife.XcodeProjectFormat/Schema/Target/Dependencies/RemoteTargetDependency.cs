using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A dependency on a target in another project.
/// </summary>
public sealed class RemoteTargetDependency : TargetDependency
{
	public RemoteTargetDependency(GroupTreeReference project, string target, ObjectId targetId)
	{
		Project = project ?? throw new ArgumentNullException(nameof(project));
		Target = target ?? throw new ArgumentNullException(nameof(target));
		TargetId = targetId ?? throw new ArgumentNullException(nameof(targetId));
	}

	/// <summary>
	/// The reference to the other project in the group tree.
	/// </summary>
	public GroupTreeReference Project { get; set; }

	public string Target { get; set; }

	public ObjectId TargetId { get; set; }

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container)
	{
		container.Encode("project", Project);
		container.Encode("target", Target);
		container.Encode("target-id", TargetId);
	}
}
