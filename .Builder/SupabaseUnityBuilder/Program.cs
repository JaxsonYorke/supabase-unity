using System.Diagnostics;

string appDir = Environment.CurrentDirectory;
var repoDir = Directory.GetParent(appDir).Parent.Parent.Parent.Parent;
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



// create main package
string packageJsonTemplate = @"
{
	""name"": ""name_replace"",
	""version"": ""1.1.0""
}";
await File.WriteAllTextAsync(Path.Combine(unityDirPath, "package.json"), packageJsonTemplate.Replace("name_replace", "com.supabase.unity"));

string asmdefTemplate = @"{
""name"": ""name_replace"",
""rootNamespace"": """",
""references"": [],
""includePlatforms"": [],
""excludePlatforms"": [],
""allowUnsafeCode"": false,
""overrideReferences"": false,
""precompiledReferences"": [],
""autoReferenced"": true,
""defineConstraints"": [],
""noEngineReferences"": false
}";
await File.WriteAllTextAsync(Path.Combine(unityDirPath, "Supabase.asmdef"), asmdefTemplate.Replace("name_replace", "Supabase"));


// create dlls packages
var buildDir = new DirectoryInfo(Path.Combine(repoDir.FullName, ".build"));


var unityDllsPath = Path.Combine(repoDir.FullName, ".UnityDlls");
var runtimeDirPath = Path.Combine(unityDirPath, "Runtime");
if (Directory.Exists(unityDllsPath))
{
	// Directory.Delete(unityDllsPath, true);
	var filesToDelete = Directory.GetFiles(unityDllsPath, "*.dll", SearchOption.AllDirectories);
	foreach(string delPath in filesToDelete)
	{
		File.Delete(delPath);
	}
}

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