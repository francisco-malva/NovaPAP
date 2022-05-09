﻿namespace AssetPipeline;

public static class Utilities
{
    public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (!recursive) return;
        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }

    public static List<string> GetAllFilesInDirectory(string path)
    {
        var files = new List<string>();

        void AddFiles(DirectoryInfo info, List<string> fileList)
        {
            fileList.AddRange(info.GetFiles().Select(file => file.FullName));
            foreach (var directory in info.GetDirectories())
            {
                AddFiles(directory, fileList);
            }
        }
        
        var directory = new DirectoryInfo(path);
        
        AddFiles(directory, files);

        return files;
    }
}