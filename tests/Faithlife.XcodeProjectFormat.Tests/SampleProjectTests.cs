using Faithlife.XcodeProjectFormat.Schema;

namespace Faithlife.XcodeProjectFormat.Tests;

internal sealed class SampleProjectTests
{
	public const string SourceDirectoryEnvironmentVariable = "XC_PROJECT_FORMAT_TEST_PROJECT_SOURCE_DIR";

	[Test]
	public void RoundTripSampleProjects()
	{
		var sourceDirectory = Environment.GetEnvironmentVariable(SourceDirectoryEnvironmentVariable);
		if (string.IsNullOrEmpty(sourceDirectory))
			Assert.Ignore($"Set {SourceDirectoryEnvironmentVariable} to a directory containing .xcproj files to run this test.");

		var paths = Directory.GetFiles(sourceDirectory, "*.xcproj", SearchOption.AllDirectories);
		Assert.That(paths, Is.Not.Empty, $"No .xcproj files found in “{sourceDirectory}”");

		using (Assert.EnterMultipleScope())
		{
			foreach (var path in paths)
			{
				var bytes = File.ReadAllBytes(path);
				var project = Project.FromJson(bytes);
				Assert.That(Project.FromJson(project.ToJsonUtf8()).ToJson(), Is.EqualTo(project.ToJson()), path);
			}
		}
	}
}
