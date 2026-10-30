using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A capability that a tool must support in order to read a project.
/// </summary>
/// <param name="CapabilityDescription">A description of the capability that is suitable for presenting to the user
/// (for example, "glow in the dark").</param>
public sealed record Capability(string CapabilityDescription) : IXCJsonStringCodable<Capability>, IComparable<Capability>
{
	public static readonly Capability KnownCapabilityForTesting = new("known capability for testing");

	public static IReadOnlyList<Capability> KnownCapabilities { get; } = [KnownCapabilityForTesting];

	public string CapabilityDescription { get; init; } = CapabilityDescription ?? throw new ArgumentNullException(nameof(CapabilityDescription));

	public bool IsSatisfied => KnownCapabilities.Contains(this);

	public int CompareTo(Capability? other) => XCJsonCoding.CompareStrings(CapabilityDescription, other?.CapabilityDescription);

	public static bool operator <(Capability? left, Capability? right) => Comparer<Capability>.Default.Compare(left, right) < 0;

	public static bool operator <=(Capability? left, Capability? right) => Comparer<Capability>.Default.Compare(left, right) <= 0;

	public static bool operator >(Capability? left, Capability? right) => Comparer<Capability>.Default.Compare(left, right) > 0;

	public static bool operator >=(Capability? left, Capability? right) => Comparer<Capability>.Default.Compare(left, right) >= 0;

	public override string ToString() => CapabilityDescription;

	string IXCJsonStringEncodable.EncodableStringRepresentation => CapabilityDescription;

	static Capability IXCJsonStringCodable<Capability>.FromEncodableStringRepresentation(string value) => new(value);
}
