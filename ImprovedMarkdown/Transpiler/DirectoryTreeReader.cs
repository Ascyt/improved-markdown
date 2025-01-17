using ImprovedMarkdown.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class DirectoryTreeReader
    {
        public static HashSet<string> ReadDirectoryTree(string directoryPath)
        {
            // Create a HashSet to store the relative paths of .md files without extensions
            HashSet<string> mdFiles = new HashSet<string>();

            // Check if the directory exists
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
            }

            // Use a recursive helper function to populate the HashSet
            FindMarkdownFiles(directoryPath, directoryPath, mdFiles);

            return mdFiles;
        }

        private static void FindMarkdownFiles(string rootPath, string currentPath, HashSet<string> mdFiles)
        {
            // Get all .md files in the current directory
            string[] files = Directory.GetFiles(currentPath, "*.md");

            foreach (var file in files)
            {
                // Get the relative path by removing the root path from the full path
                string relativePath = file.Substring(rootPath.Length).Replace("\\", "/");

                // Remove the file extension
                string relativePathWithoutExtension = Path.Combine(
                    Path.GetDirectoryName(relativePath) ?? string.Empty,
                    Path.GetFileNameWithoutExtension(relativePath)
                ).Replace("\\", "/");

                mdFiles.Add(relativePathWithoutExtension);
            }

            // Recursively search in subdirectories
            string[] directories = Directory.GetDirectories(currentPath);
            foreach (var directory in directories)
            {
                FindMarkdownFiles(rootPath, directory, mdFiles);
            }
        }
    }
}
