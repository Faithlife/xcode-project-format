namespace Faithlife.XcodeProjectFormat.Serialization;

internal sealed record XCJsonEncodingOptions
{
	public static readonly XCJsonEncodingOptions Default = new();

	public bool AddTrailingNewline { get; init; } = true;
}
