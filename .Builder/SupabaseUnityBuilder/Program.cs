using System.Diagnostics;

string appDir = Environment.CurrentDirectory;
var repoDir = new DirectoryInfo(appDir);
for (var i = 0; i < 5; i++)
{
	repoDir = repoDir.Parent
		?? throw new InvalidOperationException($"Unable to locate repository root from '{appDir}'.");
}
var submodulesDirPath = Path.Combine(repoDir.FullName, ".Submodules");
if(!Directory.Exists(submodulesDirPath))
	throw new Exception($"Something went wrong, directory doesn't exists: {submodulesDirPath}");

var unityDirPath = Path.Combine(repoDir.FullName, "Unity");
var unityDir = new DirectoryInfo(unityDirPath);
unityDir.Create();


DirectoryInfo submodulesDir = new DirectoryInfo(submodulesDirPath);


// build dlls
DirectoryInfo supaDir = new DirectoryInfo(Path.Combine(submodulesDir.FullName, "supabase-csharp", "Supabase"));
Process cmd = new Process()
{
	StartInfo = new ProcessStartInfo("cmd.exe", "/c " + @"dotnet publish -o ../../../.build")
	{
		RedirectStandardOutput = true,
		CreateNoWindow = true,
		UseShellExecute = false,
		WorkingDirectory = supaDir.FullName
	}
};
cmd.Start();

await cmd.WaitForExitAsync();
Console.WriteLine(cmd.StandardOutput.ReadToEnd());
if (cmd.ExitCode != 0)
	throw new Exception($"dotnet publish failed with exit code {cmd.ExitCode}.");



// create main package
string packageJsonTemplate = @"
{
	""name"": ""name_replace"",
	""version"": ""1.1.0"",
	""displayName"": ""Supabase for Unity"",
	""description"": ""Self-contained Supabase C# client for Unity."",
	""unity"": ""2021.3"",
	""author"": {
		""name"": ""Jaxson Yorke"",
		""url"": ""https://github.com/JaxsonYorke/supabase-unity""
	}
}";
await File.WriteAllTextAsync(Path.Combine(unityDirPath, "package.json"), packageJsonTemplate.Replace("name_replace", "com.supabase.unity"));


// Replace the bundled runtime so removed dependencies cannot remain from an older build.
var buildDir = new DirectoryInfo(Path.Combine(repoDir.FullName, ".build"));
var runtimeDirPath = Path.Combine(unityDirPath, "Runtime");
if (Directory.Exists(runtimeDirPath))
	Directory.Delete(runtimeDirPath, true);
Directory.CreateDirectory(runtimeDirPath);

foreach(var file in buildDir.EnumerateFiles("*.dll"))
{
	var destination = Path.Combine(runtimeDirPath, file.Name);
	file.CopyTo(destination, true);
	await File.WriteAllTextAsync(destination + ".meta", CreatePluginMeta());
}

static string CreatePluginMeta() =>
@$"fileFormatVersion: 2
guid: {Guid.NewGuid():N}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 1
  isExplicitlyReferenced: 0
  validateReferences: 1
  platformData:
  - first:
      Any: 
    second:
      enabled: 1
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 0
      settings:
        DefaultValueInitialized: true
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";