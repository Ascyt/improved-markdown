using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class OutputWriter
    {
        public static async Task WriteDirectoryTreeAsync(this DirectoryNode root, string outputDir, bool useHttpPaths)
        {
            // Ensure the output directory exists, and clear it if it does
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            // Start the recursive writing process
            await WriteNodeAsync(root, outputDir, outputDir, useHttpPaths);
        }

        private static async Task WriteNodeAsync(DirectoryNode node, string currentPath, string outputRootDir, bool useHttpPaths)
        {
            List<Task> tasks = new();

            // Write all files in the current directory
            foreach (var file in node.Files)
            {
                string filePath = Path.Combine(currentPath, file.Key) + ".html";
                string contents = useHttpPaths ? HtmlUrlConverter.ConvertLocalLinksToHttp(file.Value, filePath, outputRootDir) : file.Value;

                tasks.Add(File.WriteAllTextAsync(filePath, contents));
            }

            // Recursively write all subdirectories
            foreach (var directory in node.Directories)
            {
                string directoryPath = Path.Combine(currentPath, directory.Key);
                Directory.CreateDirectory(directoryPath);
                tasks.Add(WriteNodeAsync(directory.Value, directoryPath, outputRootDir, useHttpPaths));
            }

            await Task.WhenAll(tasks);
        }
    }
}
