namespace Faithlife.XcodeProjectFormat;

/// <summary>
/// The exception thrown when an Xcode project cannot be read or written.
/// </summary>
public sealed class XcodeProjectFormatException : Exception
{
	public XcodeProjectFormatException()
	{
	}

	public XcodeProjectFormatException(string message)
		: base(message)
	{
	}

	public XcodeProjectFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public XcodeProjectFormatException(string message, string? recoverySuggestion)
		: base(message)
	{
		RecoverySuggestion = recoverySuggestion;
	}

	private XcodeProjectFormatException(XcodeProjectFormatException original, string codingPath)
		: base(original.Message, original.InnerException)
	{
		RecoverySuggestion = original.RecoverySuggestion;
		CodingPath = codingPath;
	}

	/// <summary>
	/// The path (for example, <c>/targets[0]/name</c>) of the JSON value that caused the error, if known.
	/// </summary>
	public string? CodingPath { get; }

	/// <summary>
	/// A suggestion for how the user might resolve the error, if any.
	/// </summary>
	public string? RecoverySuggestion { get; }

	internal XcodeProjectFormatException WithCodingPath(string codingPath) => new(this, codingPath);
}
