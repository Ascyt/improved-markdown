using ImprovedMarkdown.Transpiler;
using ImprovedMarkdown.Transpiler.Entities;

string filePath;

if (args.Length == 0)
{
    Console.Write("Enter file path: ");
    filePath = Console.ReadLine()!;
}
else
{
     filePath = string.Join(" ", args);
}

if (!File.Exists(filePath))
{
    Console.WriteLine($"Error: File {filePath} does not exist.");
}
    
List<SplitData> data = (await RecursiveFileReader.ReadFileRecursivelyAsync(filePath))
    .SplitFilesByParts();

_ = 0; // so it lets me set a breakpoint