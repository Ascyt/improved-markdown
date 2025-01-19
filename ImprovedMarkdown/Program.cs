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

string boilerplateDir = pArgs.Boilerplate ?? Config.BOILERPLATE_FILE;
string boilerplate;
if (!File.Exists(boilerplateDir))
{
    Console.WriteLine($"Error: Boilerplate file {boilerplateDir} does not exist.");
    return 1;
}
boilerplate = await File.ReadAllTextAsync(boilerplateDir);

string indexBoilerplateDir = pArgs.IndexBoilerplate ?? Config.INDEX_BOILERPLATE_FILE;
string indexBoilerplate;
if (!File.Exists(indexBoilerplateDir))
{
Console.WriteLine($"Error: Index boilerplate file {indexBoilerplateDir} does not exist.");
return 1;
}
indexBoilerplate = await File.ReadAllTextAsync(indexBoilerplateDir);

string inputDir = Path.GetFullPath(pArgs.InputDir);
string outputDir = Path.GetFullPath(pArgs.OutputDir);

try
{
    var tasks = DirectoryTreeReader.ReadDirectoryTree(inputDir)
        .Select(async s => new
        {
            Key = s,
            Value = (await RecursiveFileReader.ReadFileRecursivelyAsync(Path.Join(inputDir, s.TrimStart('/')) + ".md", s))
                .SplitFilesByParts()
                .FormatParagraphs()
                .BuildHtmlComponents(outputDir)
                .InjectInto(boilerplate)
        });

    var results = await Task.WhenAll(tasks);

    DirectoryNode rootDir = results
        .ToDictionary(x => x.Key, x => x.Value)
        .BuildDirectoryTree();

    await rootDir.WriteDirectoryTreeAsync(outputDir, pArgs.ServerBuild);
    await rootDir.WriteIndexFilesAsync(outputDir, indexBoilerplate, pArgs.ServerBuild);

    await inputDir.CopyResourcesRecursivelyTo(outputDir);
}
catch (SyntaxException e)
{
    e.Print();
    return -1;
}


//File.WriteAllText(outputDir, output);

DirectoryInfo directoryInfo = new DirectoryInfo(outputDir);

if (directoryInfo.Exists)
{
    Console.WriteLine($"Compiled markdown files to `{directoryInfo.FullName}`");
}

return 0;