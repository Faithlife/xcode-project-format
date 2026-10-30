using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The localizations supported by a project.
/// </summary>
public sealed class ProjectLocalizationInfo : IEquatable<ProjectLocalizationInfo>, IXCJsonEncodable, IXCJsonDecodable<ProjectLocalizationInfo>
{
	/// <param name="development">The development language.</param>
	/// <param name="supported">The other supported languages; must not include the development language.</param>
	public ProjectLocalizationInfo(Language development, IEnumerable<Language>? supported = null)
	{
		Development = development ?? throw new ArgumentNullException(nameof(development));
		Supported = new HashSet<Language>(supported ?? []);
		if (Supported.Contains(development))
			throw new ArgumentException("The supported languages must not include the development language.", nameof(supported));
	}

	public Language Development { get; }

	public IReadOnlySet<Language> Supported { get; }

	public IReadOnlySet<Language> AllLanguages => new HashSet<Language>(Supported) { Development };

	public bool Equals(ProjectLocalizationInfo? other) => other is not null && Development == other.Development && Supported.SetEquals(other.Supported);

	public override bool Equals(object? obj) => obj is ProjectLocalizationInfo other && Equals(other);

	public override int GetHashCode() => HashCode.Combine(Development, Supported.Count);

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		container.Encode("development", Development);
		container.EncodeIfNotEmpty("supported", Supported);
	}

	static ProjectLocalizationInfo IXCJsonDecodable<ProjectLocalizationInfo>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		return new ProjectLocalizationInfo(container.Decode<Language>("development"), container.Decode("supported", new HashSet<Language>()), isDecoding: true);
	}

	// decoding doesn't validate that the supported languages exclude the development language
	private ProjectLocalizationInfo(Language development, HashSet<Language> supported, bool isDecoding)
	{
		Development = development;
		Supported = supported;
		_ = isDecoding;
	}
}
