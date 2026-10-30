using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The kind of location that a file path is relative to.
/// </summary>
public enum FilePathBaseKind
{
	/// <summary>
	/// The path is absolute.
	/// </summary>
	Absolute,

	/// <summary>
	/// The path is relative to the enclosing group.
	/// </summary>
	Group,

	/// <summary>
	/// The path is relative to the project directory (<c>&lt;PROJECT&gt;</c>).
	/// </summary>
	Project,

	/// <summary>
	/// The path is relative to the developer directory (<c>&lt;DEVELOPER&gt;</c>).
	/// </summary>
	Developer,

	/// <summary>
	/// The path is relative to the build products directory (<c>&lt;PRODUCTS&gt;</c>).
	/// </summary>
	BuildProducts,

	/// <summary>
	/// The path is relative to the SDK directory (<c>&lt;SDK&gt;</c>).
	/// </summary>
	Sdk,

	/// <summary>
	/// The path is relative to a user-defined source tree build setting (<c>&lt;USER:NAME&gt;</c>).
	/// </summary>
	SourceRoot,
}

/// <summary>
/// The location that a file path is relative to.
/// </summary>
public readonly record struct FilePathBase
{
	public static FilePathBase Absolute => new(FilePathBaseKind.Absolute, null);

	public static FilePathBase Group => new(FilePathBaseKind.Group, null);

	public static FilePathBase Project => new(FilePathBaseKind.Project, null);

	public static FilePathBase Developer => new(FilePathBaseKind.Developer, null);

	public static FilePathBase BuildProducts => new(FilePathBaseKind.BuildProducts, null);

	public static FilePathBase Sdk => new(FilePathBaseKind.Sdk, null);

	public static FilePathBase SourceRoot(string variable) => new(FilePathBaseKind.SourceRoot, variable ?? throw new ArgumentNullException(nameof(variable)));

	public FilePathBaseKind Kind { get; }

	/// <summary>
	/// The name of the source tree build setting, if <see cref="Kind"/> is <see cref="FilePathBaseKind.SourceRoot"/>; otherwise null.
	/// </summary>
	public string? SourceRootVariable { get; }

	internal string? BuiltInExpansionVariable => Kind switch
	{
		FilePathBaseKind.Project => "PROJECT",
		FilePathBaseKind.Developer => "DEVELOPER",
		FilePathBaseKind.BuildProducts => "PRODUCTS",
		FilePathBaseKind.Sdk => "SDK",
		_ => null,
	};

	internal static FilePathBase? FromExpansionVariable(string variable) => variable switch
	{
		"PROJECT" => Project,
		"DEVELOPER" => Developer,
		"PRODUCTS" => BuildProducts,
		"SDK" => Sdk,
		_ => null,
	};

	private FilePathBase(FilePathBaseKind kind, string? sourceRootVariable)
	{
		Kind = kind;
		SourceRootVariable = sourceRootVariable;
	}
}

/// <summary>
/// The path of a file system item, relative to a base location.
/// </summary>
public sealed record FilePath : IXCJsonInlineKeyedEncodable, IXCJsonInlineKeyedDecodable<FilePath>
{
	/// <exception cref="XcodeProjectFormatException">The path is absolute (starts with <c>/</c> or <c>~</c>) but the base isn't, or vice versa.</exception>
	public FilePath(FilePathBase @base, string path)
	{
		ArgumentNullException.ThrowIfNull(path);
		VerifyAbsoluteness(@base, path);
		Base = @base;
		Path = path;
	}

	/// <summary>
	/// Creates a file path from its string representation (for example, <c>&lt;PROJECT&gt;/Sources</c> or <c>Sources/main.swift</c>).
	/// </summary>
	/// <exception cref="XcodeProjectFormatException">The value isn't a valid file path.</exception>
	public FilePath(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		FilePathBase @base;
		string path;
		if (value.StartsWith(c_sourceRootPrefix, StringComparison.Ordinal))
		{
			var remaining = value[c_sourceRootPrefix.Length..];
			var unescaped = XCJsonCoding.UnescapeUntilError(remaining, '>', out var rest, out var error);
			switch (error)
			{
				case XCJsonCoding.UnescapeError.UnescapedSequence:
					if (!rest.StartsWith('/'))
						throw new XcodeProjectFormatException($"Invalid filePathEncoding {value.SmartQuoted()} - missing initial path separator");
					@base = FilePathBase.SourceRoot(unescaped);
					path = rest[1..];
					break;
				case XCJsonCoding.UnescapeError.InvalidEscapeSequence:
					throw new XcodeProjectFormatException($"Invalid escape sequence {("\\" + rest).SmartQuoted()} in {value.SmartQuoted()}");
				default:
					throw new XcodeProjectFormatException($"Invalid filePathEncoding {value.SmartQuoted()}");
			}
		}
		else if (value.StartsWith('<'))
		{
			var endIndex = value.IndexOf(">/", StringComparison.Ordinal);
			if (endIndex < 0)
				throw new XcodeProjectFormatException("Unterminated path base");
			var expansionVariable = value[1..endIndex];
			@base = FilePathBase.FromExpansionVariable(expansionVariable) ?? throw new XcodeProjectFormatException($"Invalid path base: {expansionVariable.SmartQuoted()}");
			path = value[(endIndex + 2)..];
		}
		else
		{
			@base = value.StartsWith('/') ? FilePathBase.Absolute : FilePathBase.Group;
			path = value.UnescapingCharacter('<');
		}

		VerifyAbsoluteness(@base, path);
		Base = @base;
		Path = path;
	}

	public static FilePath FromString(string value) => new(value);

	public static implicit operator FilePath(string value) => FromString(value);

	public FilePathBase Base { get; }

	public string Path { get; }

	/// <summary>
	/// The last component of the path, as returned by <c>NSString.lastPathComponent</c>.
	/// </summary>
	public string LastPathComponent
	{
		get
		{
			var path = Path;
			if (path.Length > 1)
				path = path.TrimEnd('/');
			if (path.Length == 0)
				return path.Length == Path.Length ? "" : "/";
			var index = path.LastIndexOf('/');
			return index < 0 || path.Length == 1 ? path : path[(index + 1)..];
		}
	}

	public override string ToString()
	{
		if (Base.BuiltInExpansionVariable is { } expansionVariable)
			return "<" + expansionVariable + ">/" + Path;
		if (Base.SourceRootVariable is { } sourceRootVariable)
			return c_sourceRootPrefix + sourceRootVariable.EscapingCharacter('>') + ">/" + Path;
		return Path.EscapingCharacter('<');
	}

	void IXCJsonInlineKeyedEncodable.Encode(XCJsonEncoder.KeyedContainer container) => container.EncodeIfNotDefault("path", ToString(), "");

	static FilePath IXCJsonInlineKeyedDecodable<FilePath>.Decode(XCJsonDecoder.KeyedContainer container) => new(container.Decode("path", ""));

	private static void VerifyAbsoluteness(FilePathBase @base, string path)
	{
		var isAbsolutePath = path.StartsWith('/') || path.StartsWith('~');
		if (isAbsolutePath != (@base.Kind == FilePathBaseKind.Absolute))
			throw new XcodeProjectFormatException($"Base disagrees with absoluteness of path: {path}");
	}

	private const string c_sourceRootPrefix = "<USER:";
}
