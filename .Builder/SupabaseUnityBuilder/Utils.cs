public class Utils
{
	public static int CopyDirectoryRecursive(DirectoryInfo sourceDir, string destinationDir)
	{
		Directory.CreateDirectory(destinationDir);
	
		int totalFilesCopied = 0;
	
		foreach(FileInfo file in sourceDir.EnumerateFiles())
		{
			if(file.Name.StartsWith("."))
				continue;
		
			if(!file.Name.EndsWith("cs") )
				continue;
			string targetFilePath = Path.Combine(destinationDir, file.Name);
			file.CopyTo(targetFilePath);
			totalFilesCopied++;
		}

		foreach (DirectoryInfo subDir in sourceDir.EnumerateDirectories())
		{
			if (subDir.Name.StartsWith("."))
				continue;

			if (subDir.Name.Equals("obj", StringComparison.OrdinalIgnoreCase)
				|| subDir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase))
				continue;

			if (subDir.Name.Contains("Test", StringComparison.OrdinalIgnoreCase)
				|| subDir.Name.Contains("Example", StringComparison.OrdinalIgnoreCase))
				continue;

			string newDir = Path.Combine(destinationDir, subDir.Name);
			totalFilesCopied += CopyDirectoryRecursive(subDir, newDir);
		}
		// if(totalFilesCopied == 0)
			// Directory.Delete(destinationDir);
	
		return totalFilesCopied;
	}
}