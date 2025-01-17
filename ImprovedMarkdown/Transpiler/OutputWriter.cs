using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class OutputWriter
    { 
        public static async Task WriteDirectoryTree(this Dictionary<string, string> files, string outputDir)
        {
            // Ensure the output directory exists, and clear it if it does
            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            // Iterate over each file entry in the dictionary
            foreach (var entry in files)
            {
                // Construct the full path for the file
                string relativePath = entry.Key.TrimStart('/');
                string fullPath = Path.Combine(outputDir, relativePath) + ".html";

                // Ensure the directory for the file exists
                string directoryPath = Path.GetDirectoryName(fullPath)!;
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the file content asynchronously
                await File.WriteAllTextAsync(fullPath, entry.Value);
            }
        }
    }
}
