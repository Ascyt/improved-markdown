using ImprovedMarkdown.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class IndexWriter
    {
        public static async Task WriteIndexFiles(this DirectoryNode root, string outputDir, string indexBoilerplateHtml)
        {
            // Ensure the output directory exists
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Start the recursive index file writing process
            await WriteIndexFileAsync(root, outputDir, indexBoilerplateHtml);
        }

        private static async Task WriteIndexFileAsync(DirectoryNode node, string currentPath, string indexBoilerplateHtml)
        {
            // Create the HTML list for files
            string filesList = "<ul>" + string.Join("", node.Files.Keys.Select(f => $"<li><a href=\"{f}.html\">{f}</a></li>")) + "</ul>";

            // Create the HTML list for directories
            string directoriesList = "<ul>" + string.Join("", node.Directories.Keys.Select(d => $"<li><a href=\"{d}/index.html\">{d}</a></li>")) + "</ul>";

            var indexComponents = new HtmlIndexComponents(
                title: $"Index of {node.Name}",
                files: filesList,
                dictionaries: directoriesList
            );

            string indexHtmlContent = indexComponents.InjectInto(indexBoilerplateHtml);

            // Write the index.html file
            string indexPath = Path.Combine(currentPath, "index.html");
            await File.WriteAllTextAsync(indexPath, indexHtmlContent);

            // Recursively create index files for subdirectories
            foreach (var directory in node.Directories)
            {
                string directoryPath = Path.Combine(currentPath, directory.Key);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await WriteIndexFileAsync(directory.Value, directoryPath, indexBoilerplateHtml);
            }
        }
    }
}
