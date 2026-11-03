using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// The visibility of a header in a "Copy Headers" build phase.
/// </summary>
public enum HeaderRole
{
	[XCJsonName("public")]
	Public,

	[XCJsonName("private")]
	Private,
}

/// <summary>
/// The Mach Interface Generator (MIG) code to generate for a <c>.defs</c> file.
/// </summary>
public enum MachInterfaceGeneration
{
	[XCJsonName("client")]
	Client,

	[XCJsonName("server")]
	Server,

	[XCJsonName("both")]
	Both,
}

/// <summary>
/// The visibility of generated code (for example, for Core Data models or Intents definitions).
/// </summary>
public enum CodeGenerationVisibility
{
	[XCJsonName("public")]
	Public,

	[XCJsonName("private")]
	Private,

	[XCJsonName("project")]
	Project,
}

/// <summary>
/// Whether code should be generated for a file.
/// </summary>
public enum CodeGeneration
{
	[XCJsonName("default")]
	Default,

	[XCJsonName("skip")]
	Skip,
}

/// <summary>
/// Whether headers are kept or removed when a framework is copied.
/// </summary>
public enum HeaderPreservation
{
	[XCJsonName("keep")]
	Keep,

	[XCJsonName("remove-on-copy")]
	RemoveOnCopy,
}

/// <summary>
/// Attributes of a build file.
/// </summary>
public sealed class BuildFileAttributes : IXCJsonInlineKeyedEncodable, IXCJsonInlineKeyedDecodable<BuildFileAttributes>
{
	public HeaderRole? HeaderRole { get; set; }

	public MachInterfaceGeneration? MachInterfaceGeneration { get; set; }

	public bool IsWeak { get; set; }

	public bool CodeSignOnCopy { get; set; }

	public CodeGeneration CodeGeneration { get; set; }

	public HeaderPreservation HeaderPreservation { get; set; }

	public bool Decompress { get; set; }

	public CodeGenerationVisibility? CodeGenerationVisibility { get; set; }

	/// <summary>
	/// True if every attribute has its default value.
	/// </summary>
	public bool IsDefault => HeaderRole is null && MachInterfaceGeneration is null && !IsWeak && !CodeSignOnCopy &&
		CodeGeneration == CodeGeneration.Default && HeaderPreservation == HeaderPreservation.Keep && !Decompress && CodeGenerationVisibility is null;

	void IXCJsonInlineKeyedEncodable.Encode(XCJsonEncoder.KeyedContainer container)
	{
		container.EncodeIfNotNull("header-role", HeaderRole);
		container.EncodeIfNotNull("mach-interface-generation", MachInterfaceGeneration);
		container.EncodeIfNotDefault("is-weak", IsWeak, false);
		container.EncodeIfNotDefault("code-sign-on-copy", CodeSignOnCopy, false);
		container.EncodeIfNotDefault("code-generation", CodeGeneration, CodeGeneration.Default);
		container.EncodeIfNotDefault("header-preservation", HeaderPreservation, HeaderPreservation.Keep);
		container.EncodeIfNotDefault("decompress", Decompress, false);
		container.EncodeIfNotNull("code-generation-visibility", CodeGenerationVisibility);
	}

	static BuildFileAttributes IXCJsonInlineKeyedDecodable<BuildFileAttributes>.Decode(XCJsonDecoder.KeyedContainer container) => new()
	{
		HeaderRole = container.DecodeOptionalValue<HeaderRole>("header-role"),
		MachInterfaceGeneration = container.DecodeOptionalValue<MachInterfaceGeneration>("mach-interface-generation"),
		IsWeak = container.Decode("is-weak", false),
		CodeSignOnCopy = container.Decode("code-sign-on-copy", false),
		CodeGeneration = container.Decode("code-generation", CodeGeneration.Default),
		HeaderPreservation = container.Decode("header-preservation", HeaderPreservation.Keep),
		Decompress = container.Decode("decompress", false),
		CodeGenerationVisibility = container.DecodeOptionalValue<CodeGenerationVisibility>("code-generation-visibility"),
	};
}
