using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A dependency on another target in the same project.
/// </summary>
public sealed class LocalTargetDependency : TargetDependency
{
	public LocalTargetDependency(LocalTargetReference target) => Target = target ?? throw new ArgumentNullException(nameof(target));

	public LocalTargetReference Target { get; set; }

	private protected override void EncodeFields(XCJsonEncoder.KeyedContainer container) => container.Encode("target", Target);
}
