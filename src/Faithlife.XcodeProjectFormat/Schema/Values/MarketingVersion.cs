using System.Globalization;
using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A version number such as <c>5.0</c> or <c>5.10.1</c>.
/// </summary>
public readonly record struct MarketingVersion(int Major, int Minor, int Update = 0) : IXCJsonStringCodable<MarketingVersion>
{
	public static MarketingVersion Parse(string value)
	{
		var components = value.Split('.');
		if (components.Length is >= 2 and <= 3)
		{
			var numbers = new int[components.Length];
			var isValid = true;
			for (var index = 0; index < components.Length && isValid; index++)
				isValid = int.TryParse(components[index], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out numbers[index]);
			if (isValid)
				return new MarketingVersion(numbers[0], numbers[1], numbers.Length >= 3 ? numbers[2] : 0);
		}

		throw new XcodeProjectFormatException($"Invalid version string {value.SmartQuoted()}.");
	}

	public override string ToString() => Update == 0 ? string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}") : string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Update}");

	string IXCJsonStringEncodable.EncodableStringRepresentation => ToString();

	static MarketingVersion IXCJsonStringCodable<MarketingVersion>.FromEncodableStringRepresentation(string value) => Parse(value);
}
