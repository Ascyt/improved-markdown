using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;

namespace ImprovedMarkdown.Transpiler
{
    internal static class RecursiveFileReader
    {
        public static async Task<List<SplitData>> ReadFileRecursivelyAsync(string filePath, string directoryTree, Stack<ParsedFile>? upperFiles = null, SplitData? importedFrom = null)
        {
            string fileContents = (await File.ReadAllTextAsync(filePath)).Replace("\r", "");
            string[] lines = fileContents.Split('\n');
            DirectoryInfo? workingDirectory = Directory.GetParent(filePath);

            List<string> currentLines = new();
            int startCurrentLinesIndex = 0;
            // Output is split by imported files
            List<SplitData> output = new();
            upperFiles ??= new Stack<ParsedFile>();
            ParsedFile parsedFile = new(filePath, directoryTree, fileContents, upperFiles, importedFrom);

            for (int i = 0; i < lines.Length; i++)
            {
                string trimmedLine = lines[i].Trim();

                // Find imports of other files
                if (trimmedLine.StartsWith("&"))
                {
                    AddCurrentLines();

                    Stack<ParsedFile> newFileUpperFiles = new(upperFiles);
                    newFileUpperFiles.Push(parsedFile);
                    string newFilePath = trimmedLine.Substring(1).Trim();
                    SplitData newFileImportedFrom = new SyntaxTypeImport(newFilePath, parsedFile, i, 0);

                    if (workingDirectory is not null)
                    {
                        newFilePath = Path.Join(workingDirectory.FullName, newFilePath);
                    }
                    
                    output.AddRange(await ReadFileRecursivelyAsync(newFilePath, directoryTree, newFileUpperFiles, newFileImportedFrom));

                    currentLines.Clear();
                    startCurrentLinesIndex = i;

                    continue;
                }
                
                currentLines.Add(lines[i]);
            }

            AddCurrentLines();

            return output;

            void AddCurrentLines()
            {
                output.Add(new SyntaxTypeFile(string.Join("\n", currentLines), parsedFile, startCurrentLinesIndex, 0));
            }
        }
    }
}
