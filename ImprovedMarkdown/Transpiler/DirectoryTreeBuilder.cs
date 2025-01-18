using ImprovedMarkdown.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class DirectoryTreeBuilder
    {
        public static DirectoryNode BuildDirectoryTree(this Dictionary<string, string> fileDictionary)
        {
            var root = new DirectoryNode("root");

            foreach (var entry in fileDictionary)
            {
                string filePath = entry.Key;
                string fileContent = entry.Value;

                // Split the path into parts
                string[] parts = filePath.TrimStart('/').Split('/');

                // Start from the root
                DirectoryNode currentNode = root;

                // Traverse or create directories
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string dirName = parts[i];
                    if (!currentNode.Directories.ContainsKey(dirName))
                    {
                        currentNode.Directories[dirName] = new DirectoryNode(dirName);
                    }
                    currentNode = currentNode.Directories[dirName];
                }

                // Add the file to the current directory
                string fileName = parts[^1];
                currentNode.Files[fileName] = fileContent;
            }

            return root;
        }
    }
}
