using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The versions of a remote Swift package that the project accepts.
/// </summary>
public sealed record SwiftPackageVersionConstraint : IXCJsonEncodable, IXCJsonDecodable<SwiftPackageVersionConstraint>
{
	public static SwiftPackageVersionConstraint Revision(string revision) => new(SwiftPackageVersionConstraintKind.Revision, revision, null);

	public static SwiftPackageVersionConstraint Branch(string branch) => new(SwiftPackageVersionConstraintKind.Branch, branch, null);

	public static SwiftPackageVersionConstraint Version(string version) => new(SwiftPackageVersionConstraintKind.Version, version, null);

	public static SwiftPackageVersionConstraint VersionRange(string minimum, string maximum) =>
		new(SwiftPackageVersionConstraintKind.VersionRange, minimum, maximum ?? throw new ArgumentNullException(nameof(maximum)));

	public static SwiftPackageVersionConstraint UpToNextMinorVersion(string version) => new(SwiftPackageVersionConstraintKind.UpToNextMinorVersion, version, null);

	public static SwiftPackageVersionConstraint UpToNextMajorVersion(string version) => new(SwiftPackageVersionConstraintKind.UpToNextMajorVersion, version, null);

	public SwiftPackageVersionConstraintKind Kind { get; }

	/// <summary>
	/// The revision, branch, or version; or the minimum version of a version range.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// The (exclusive) maximum version of a version range.
	/// </summary>
	public string? MaximumVersion { get; }

	private SwiftPackageVersionConstraint(SwiftPackageVersionConstraintKind kind, string value, string? maximumVersion)
	{
		Kind = kind;
		Value = value ?? throw new ArgumentNullException(nameof(value));
		MaximumVersion = maximumVersion;
	}

	private static bool IsBasicVersionNumber(string value) => value.All(x => x is (>= '0' and <= '9') or '.');

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		switch (Kind)
		{
			case SwiftPackageVersionConstraintKind.Revision:
				container.Encode("revision", Value);
				break;
			case SwiftPackageVersionConstraintKind.Branch:
				container.Encode("branch", Value);
				break;
			case SwiftPackageVersionConstraintKind.Version:
				container.Encode("version", Value);
				break;
			case SwiftPackageVersionConstraintKind.UpToNextMinorVersion:
				container.Encode("up-to-next-minor-version", Value);
				break;
			case SwiftPackageVersionConstraintKind.UpToNextMajorVersion:
				container.Encode("up-to-next-major-version", Value);
				break;
			default:
				if (IsBasicVersionNumber(Value) && IsBasicVersionNumber(MaximumVersion!))
				{
					container.Encode("version-range", Value + "..<" + MaximumVersion);
				}
				else
				{
					container.Encode("version-range-min", Value);
					container.Encode("version-range-max", MaximumVersion);
				}
				break;
		}
	}

	static SwiftPackageVersionConstraint IXCJsonDecodable<SwiftPackageVersionConstraint>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		if (container.DecodeOptional<string>("revision") is { } revision)
			return Revision(revision);
		if (container.DecodeOptional<string>("branch") is { } branch)
			return Branch(branch);
		if (container.DecodeOptional<string>("version") is { } version)
			return Version(version);
		if (container.DecodeOptional<string>("up-to-next-minor-version") is { } minor)
			return UpToNextMinorVersion(minor);
		if (container.DecodeOptional<string>("up-to-next-major-version") is { } major)
			return UpToNextMajorVersion(major);
		if (container.DecodeOptional<string>("version-range") is { } range)
		{
			const string separator = "..<";
			var index = range.IndexOf(separator, StringComparison.Ordinal);
			if (index < 0 || range.IndexOf(separator, index + 1, StringComparison.Ordinal) >= 0)
				throw new XcodeProjectFormatException($"Unexpected version range value {range.SmartQuoted()}");
			return VersionRange(range[..index], range[(index + separator.Length)..]);
		}

		return VersionRange(container.Decode<string>("version-range-min"), container.Decode<string>("version-range-max"));
	}
}

/// <summary>
/// The kind of a Swift package version constraint.
/// </summary>
public enum SwiftPackageVersionConstraintKind
{
	Revision,
	Branch,
	Version,
	VersionRange,
	UpToNextMinorVersion,
	UpToNextMajorVersion,
}
