using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class ResourceCopier
    {
        public static async Task CopyResourcesRecursivelyTo(this string fromDir, string toDir)
        {
            if (!Directory.Exists(fromDir))
            {
                throw new DirectoryNotFoundException($"Source directory not found: {fromDir}");
            }

            // Ensure the target directory exists
            Directory.CreateDirectory(toDir);

            // Get all files in the current directory, excluding .md files
            var files = Directory.GetFiles(fromDir).Where(file => !file.EndsWith(".md", StringComparison.OrdinalIgnoreCase));

            // Copy each file concurrently
            var copyFileTasks = files.Select(async file =>
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(toDir, fileName);
                using (var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var destinationStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            });

            // Wait for all file copy operations to complete
            await Task.WhenAll(copyFileTasks);

            // Get all subdirectories
            var directories = Directory.GetDirectories(fromDir);

            // Recursively copy each subdirectory
            var copyDirectoryTasks = directories.Select(async subDir =>
            {
                string subDirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(toDir, subDirName);
                await CopyResourcesRecursivelyTo(subDir, destSubDir);
            });

            // Wait for all directory copy operations to complete
            await Task.WhenAll(copyDirectoryTasks);
        }
    }
}
