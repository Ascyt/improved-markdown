﻿using ImprovedMarkdown;
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
    
string output = (await RecursiveFileReader.ReadFileRecursivelyAsync(pArgs.InputFile))
    .SplitFilesByParts()
    .FormatParagraphs()
    .BuildHtmlComponents()
    .InjectInto(boilerplate);

File.WriteAllText(pArgs.OutputFile, output);

FileInfo outputFileInfo = new FileInfo(pArgs.OutputFile);

if (outputFileInfo.Exists)
{
    Console.WriteLine($"Saved file to `{outputFileInfo.FullName}` ({outputFileInfo.Length} bytes)");
}

return 0;