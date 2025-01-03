using ImprovedMarkdown.Transpiler.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class SyntaxException(Stack<ParsedFile> fileStack, int fromRow, int toRow, int fromCol, int toCol, string message) 
        : Exception(message)
    {
        public Stack<ParsedFile> FileStack { get; set; } = fileStack;
        public int FromRow { get; set; } = fromRow;
        public int ToRow { get; set; } = toRow;
        public int FromCol { get; set; } = fromCol;
        public int ToCol { get; set; } = toCol;

        public void Print()
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            ParsedFile currentFile = FileStack.First();
            string[] currentFileLines = currentFile.FullContents.Split("\n");

            Console.WriteLine($"Syntax Error:");
            Console.WriteLine($"\t{Message}");
            Console.WriteLine();
            Console.WriteLine($"In file: {currentFile.FilePath}");
            Console.WriteLine();

            string rowString = FromRow == ToRow ? $"row {FromRow}" : $"rows {FromRow}-{ToRow}";
            string colString = FromCol == ToCol ? $"column {FromCol}" : $"columns {FromCol}-{ToCol}";
            Console.WriteLine($"At {rowString}, {colString}:");

            StringBuilder errorLocation = new();
            for (int row = FromRow; row <= ToRow; row++)
            {
                string line = currentFileLines[row - 1];

                errorLocation.AppendLine("\t" + line);
                errorLocation.Append("\t");

                if (row == FromRow)
                {
                    for (int col = 0; col < FromCol - 2; col++)
                    {
                        errorLocation.Append(" ");
                    }

                    int until = row == ToRow ? ToCol : line.Length + 1;
                    for (int col = FromCol; col <= until; col++)
                    {
                        errorLocation.Append("~");
                    }

                    errorLocation.AppendLine();
                    continue;
                }
                if (row == ToRow)
                {
                    for (int col = 0; col < ToCol - 1; col++)
                    {
                        errorLocation.Append("~");
                    }
                    errorLocation.AppendLine();
                    continue;
                }

                errorLocation.AppendLine("\t" + "~".Repeat(line.Length));
            }
            Console.WriteLine(errorLocation);

            Console.WriteLine("File stack:");
            foreach (ParsedFile file in FileStack)
            {
                Console.WriteLine($"\tFile: {file.FilePath}");
                if (file.ImportedFrom is not null)
                    Console.WriteLine($"\t\tImported in: {file.ImportedFrom.File.FilePath}; row {file.ImportedFrom.rowIndex}, column {file.ImportedFrom.colIndex}");
                else
                    Console.WriteLine("\t\tRoot file");
            }

            Console.ForegroundColor = previousColor;
        }
    }
}
