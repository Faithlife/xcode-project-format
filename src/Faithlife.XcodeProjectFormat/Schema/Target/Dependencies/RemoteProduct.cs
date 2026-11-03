using Faithlife.XcodeProjectFormat.Serialization;

namespace Faithlife.XcodeProjectFormat.Schema;

/// <summary>
/// A product of a target in another project that is referenced by this project.
/// </summary>
public sealed class RemoteProduct : IXCJsonEncodable, IXCJsonDecodable<RemoteProduct>
{
	public RemoteProduct(GroupTreeReference project, string target, ObjectId productId, string path)
	{
		Project = project ?? throw new ArgumentNullException(nameof(project));
		Target = target ?? throw new ArgumentNullException(nameof(target));
		ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
		Path = path ?? throw new ArgumentNullException(nameof(path));
	}

	public GroupTreeReference Project { get; set; }

	public string Target { get; set; }

	public ObjectId ProductId { get; set; }

	public string Path { get; set; }

	public FileTypeId? FileType { get; set; }

	public List<ProjectBuildFile> BuildFiles { get; set; } = [];

	// Matches NSString.pathComponents.count == 1.
	private bool PathIsName => Path == "/" || (Path.Length != 0 && !Path.Contains('/', StringComparison.Ordinal));

	void IXCJsonEncodable.Encode(XCJsonEncoder encoder)
	{
		var container = encoder.OpenKeyedContainer();
		container.Encode(PathIsName ? "name" : "path", Path);
		container.Encode("project", Project);
		container.Encode("target", Target);
		container.Encode("product-id", ProductId);
		container.EncodeIfNotNull("type", FileType);
		container.Encode("target-membership", BuildFiles);
	}

	static RemoteProduct IXCJsonDecodable<RemoteProduct>.Decode(XCJsonDecoder decoder)
	{
		var container = decoder.OpenKeyedContainer();
		var path = container.DecodeOptional<string>("name") ?? container.Decode<string>("path");
		return new RemoteProduct(container.Decode<GroupTreeReference>("project"), container.Decode<string>("target"), container.Decode<ObjectId>("product-id"), path)
		{
			FileType = container.DecodeOptional<FileTypeId>("type"),
			BuildFiles = container.Decode<List<ProjectBuildFile>>("target-membership"),
		};
	}
}
