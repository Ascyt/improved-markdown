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

            Console.WriteLine($"Syntax Error:");
            Console.WriteLine($"\t{Message}");
            Console.WriteLine();
            Console.WriteLine($"In file: {FileStack.First().FilePath}");

            string row = FromRow == ToRow ? $"row {FromRow}" : $"rows {FromRow}-{ToRow}";
            string col = FromCol == ToCol ? $"column {FromCol}" : $"columns {FromCol}-{ToCol}";

            Console.WriteLine($"At {row}, {col}");
            Console.WriteLine();
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
