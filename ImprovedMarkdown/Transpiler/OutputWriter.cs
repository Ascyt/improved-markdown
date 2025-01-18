using ImprovedMarkdown.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class OutputWriter
    {
        public static async Task WriteDirectoryTree(this DirectoryNode root, string outputDir)
        {
            // Ensure the output directory exists, and clear it if it does
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            // Start the recursive writing process
            await WriteNodeAsync(root, outputDir);
        }

        private static async Task WriteNodeAsync(DirectoryNode node, string currentPath)
        {
            List<Task> tasks = new();

            // Write all files in the current directory
            foreach (var file in node.Files)
            {
                string filePath = Path.Combine(currentPath, file.Key) + ".html";
                tasks.Add(File.WriteAllTextAsync(filePath, file.Value));
            }

            // Recursively write all subdirectories
            foreach (var directory in node.Directories)
            {
                string directoryPath = Path.Combine(currentPath, directory.Key);
                Directory.CreateDirectory(directoryPath);
                tasks.Add(WriteNodeAsync(directory.Value, directoryPath));
            }

            await Task.WhenAll(tasks);
        }
    }
}
