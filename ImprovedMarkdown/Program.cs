using ImprovedMarkdown;
using ImprovedMarkdown.Transpiler;
using ImprovedMarkdown.Transpiler.Entities;
using PowerArgs;

ProgramArgs pArgs;
try
{
    pArgs = Args.Parse<ProgramArgs>(args);
}
catch (ArgException ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<ProgramArgs>());
    return 1;
}

string boilerplate;
if (!File.Exists(Config.BOILERPLATE_FILE))
{
    Console.WriteLine($"Error: Boilerplate file {Config.BOILERPLATE_FILE} does not exist.");
    return 1;
}
boilerplate = await File.ReadAllTextAsync(Config.BOILERPLATE_FILE);

try
{
    var tasks = DirectoryTreeReader.ReadDirectoryTree(pArgs.InputDir)
        .Select(async s => new
        {
            Key = s,
            Value = (await RecursiveFileReader.ReadFileRecursivelyAsync(Path.Join(pArgs.InputDir, s.TrimStart('/')) + ".md", s))
                .SplitFilesByParts()
                .FormatParagraphs()
                .BuildHtmlComponents()
                .InjectInto(boilerplate)
        });

    var results = await Task.WhenAll(tasks);

    Dictionary<string, string> outputFiles = results.ToDictionary(x => x.Key, x => x.Value);

    await outputFiles.WriteDirectoryTree(pArgs.OutputDir);
}
catch (SyntaxException e)
{
    e.Print();
    return -1;
}


//File.WriteAllText(pArgs.OutputDir, output);

FileInfo outputFileInfo = new FileInfo(pArgs.OutputDir);

if (outputFileInfo.Exists)
{
    Console.WriteLine($"Saved file to `{outputFileInfo.FullName}` ({outputFileInfo.Length} bytes)");
}

return 0;