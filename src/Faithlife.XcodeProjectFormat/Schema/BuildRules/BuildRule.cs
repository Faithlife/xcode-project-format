using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A custom build rule of a target.
/// </summary>
public sealed class BuildRule : IXCJsonEncodable, IXCJsonDecodable<BuildRule>
{
	/// <param name="processor">The compiler specification identifier of the tool that processes files, or <c>com.apple.compilers.proxy.script</c> for a custom script.</param>
	public BuildRule(string processor) => Processor = processor ?? throw new ArgumentNullException(nameof(processor));

	public ObjectId? ObjectId { get; set; }

	public string Processor { get; set; }

	public string? Name { get; set; }

	public FileTypeId? FileType { get; set; }

	public string? FilePatterns { get; set; }

	public string? Script { get; set; }

	public List<string> InputFiles { get; set; } = [];

	public List<string> InputFileLists { get; set; } = [];

	public List<string> OutputFiles { get; set; } = [];

	public List<string> OutputFileLists { get; set; } = [];

	public List<string> OutputFilesCompilerFlags { get; set; } = [];

	public string? DependencyFile { get; set; }

	public bool RunOncePerArchitecture { get; set; } = true;

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		container.EncodeIfNotNull("name", Name);
		container.EncodeIfNotNull("id", ObjectId);
		container.Encode("processor", Processor);
		container.EncodeIfNotNull("file-type", FileType);
		container.EncodeIfNotNull("file-patterns", FilePatterns);
		container.EncodeIfNotEmpty("input-files", InputFiles);
		container.EncodeIfNotEmpty("input-file-lists", InputFileLists);
		container.EncodeIfNotEmpty("output-files", OutputFiles);
		container.EncodeIfNotEmpty("output-file-lists", OutputFileLists);
		container.EncodeIfNotEmpty("output-files-compiler-flags", OutputFilesCompilerFlags);
		container.EncodeIfNotNull("dependency-file", DependencyFile);
		container.EncodeIfNotDefault("run-once-per-architecture", RunOncePerArchitecture, true);
		container.EncodeIfNotNull("script", Script is null ? null : new MultilineText(Script));
	}

	static BuildRule IXCJsonDecodable<BuildRule>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		var name = container.DecodeOptional<string>("name");
		var objectId = container.DecodeOptional<ObjectId>("id");
		return new BuildRule(container.Decode<string>("processor"))
		{
			Name = name,
			ObjectId = objectId,
			FileType = container.DecodeOptional<FileTypeId>("file-type"),
			FilePatterns = container.DecodeOptional<string>("file-patterns"),
			InputFiles = container.Decode("input-files", new List<string>()),
			InputFileLists = container.Decode("input-file-lists", new List<string>()),
			OutputFiles = container.Decode("output-files", new List<string>()),
			OutputFileLists = container.Decode("output-file-lists", new List<string>()),
			OutputFilesCompilerFlags = container.Decode("output-files-compiler-flags", new List<string>()),
			DependencyFile = container.DecodeOptional<string>("dependency-file"),
			RunOncePerArchitecture = container.Decode("run-once-per-architecture", true),
			Script = container.DecodeOptional<MultilineText>("script")?.Text,
		};
	}
}
