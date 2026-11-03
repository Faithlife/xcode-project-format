using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The value of a build setting, which is either a string or an array of strings.
/// </summary>
public sealed class BuildSetting : IEquatable<BuildSetting>, IXCJsonEncodable, IXCJsonDecodable<BuildSetting>
{
	public BuildSetting(string value) => StringValue = value ?? throw new ArgumentNullException(nameof(value));

	public BuildSetting(IEnumerable<string> values) => ArrayValue = values?.ToList() ?? throw new ArgumentNullException(nameof(values));

	public static BuildSetting FromString(string value) => new(value);

	public static implicit operator BuildSetting(string value) => FromString(value);

	/// <summary>
	/// The string value, if the build setting is a string; otherwise null.
	/// </summary>
	public string? StringValue { get; }

	/// <summary>
	/// The array value, if the build setting is an array; otherwise null.
	/// </summary>
	public IReadOnlyList<string>? ArrayValue { get; }

	public bool IsArray => ArrayValue is not null;

	public bool Equals(BuildSetting? other) =>
		other is not null && (StringValue is not null ? StringValue == other.StringValue : other.ArrayValue is not null && ArrayValue!.SequenceEqual(other.ArrayValue));

	public override bool Equals(object? obj) => obj is BuildSetting other && Equals(other);

	public override int GetHashCode() => StringValue?.GetHashCode(StringComparison.Ordinal) ?? ArrayValue!.Aggregate(0, (hash, x) => HashCode.Combine(hash, x));

	public override string ToString() => StringValue ?? string.Join(" ", ArrayValue!);

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		if (StringValue is not null)
		{
			encoder.OpenPrimitiveContainer().Encode(StringValue);
		}
		else
		{
			var container = encoder.OpenOrdinalContainer();
			foreach (var value in ArrayValue!)
				container.Encode(value);
		}
	}

	static BuildSetting IXCJsonDecodable<BuildSetting>.Decode(XCJsonDecoder decoder) =>
		decoder.CurrentNodeType == XCJsonValueType.String ? new BuildSetting(decoder.DecodePrimitiveString()) : new BuildSetting(decoder.Decode<List<string>>());
}
