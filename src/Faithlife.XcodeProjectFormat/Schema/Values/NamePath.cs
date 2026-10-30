using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A path through the group tree, by name.
/// </summary>
public sealed class NamePath : IEquatable<NamePath>, IXCJsonEncodable, IXCJsonDecodable<NamePath>
{
	public NamePath(IEnumerable<NamePathComponent> components) => Components = components?.ToList() ?? throw new ArgumentNullException(nameof(components));

	public NamePath(params NamePathComponent[] components)
		: this((IEnumerable<NamePathComponent>) components)
	{
	}

	/// <summary>
	/// Creates a name path from child names.
	/// </summary>
	public static NamePath FromChildNames(IEnumerable<string> childNames) => new(childNames.Select(NamePathComponent.Child));

	/// <summary>
	/// Parses a slash-separated name path; <c>.</c> and <c>..</c> are treated as relative references.
	/// </summary>
	public static NamePath ParseLosslessPathRepresentation(string value) =>
		new(value.Split('/').Select(x => NamePathComponent.ParseLosslessStringEncoding(x, requiresSlashValidation: false)));

	/// <summary>
	/// Returns a name path with the components of <paramref name="right"/> appended to those of <paramref name="left"/>.
	/// </summary>
	public static NamePath Add(NamePath left, NamePath right)
	{
		ArgumentNullException.ThrowIfNull(left);
		ArgumentNullException.ThrowIfNull(right);
		return new(left.Components.Concat(right.Components));
	}

	public static NamePath operator +(NamePath left, NamePath right) => Add(left, right);

	public IReadOnlyList<NamePathComponent> Components { get; }

	/// <summary>
	/// The slash-separated representation of the path, or null if it has no components or one of its components can't be represented losslessly.
	/// </summary>
	public string? LosslessPathRepresentation
	{
		get
		{
			if (Components.Count == 0)
				return null;
			var encodings = Components.Select(x => x.LosslessStringEncoding).ToList();
			return encodings.Any(x => x is null) ? null : string.Join("/", encodings);
		}
	}

	public bool Equals(NamePath? other) => other is not null && Components.SequenceEqual(other.Components);

	public override bool Equals(object? obj) => obj is NamePath other && Equals(other);

	public override int GetHashCode() => Components.Aggregate(0, (hash, x) => HashCode.Combine(hash, x));

	public override string ToString() => string.Join("/", Components);

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (LosslessPathRepresentation is { } path)
			encoder.OpenPrimitiveContainer().Encode(path);
		else
			EncodeComponents(encoder, Components);
	}

	static NamePath IXCJsonDecodable<NamePath>.Decode(XCJsonDecoder decoder) =>
		decoder.CurrentNodeType == XCJsonValueType.String ? ParseLosslessPathRepresentation(decoder.DecodePrimitiveString()) : new NamePath(decoder.Decode<List<NamePathComponent>>());

	internal static void EncodeComponents(XCJsonEncoder encoder, IEnumerable<NamePathComponent> components)
	{
		var container = encoder.OpenOrdinalContainer();
		foreach (var component in components)
			container.Encode(component);
	}
}

/// <summary>
/// The kind of relative reference in a name path.
/// </summary>
public enum NamePathRelativeReference
{
	/// <summary>
	/// <c>.</c>
	/// </summary>
	Current,

	/// <summary>
	/// <c>..</c>
	/// </summary>
	Parent,
}
