using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal class ParsedFile(string filePath, Stack<ParsedFile> upperFiles, SplitData? importedFrom)
    {
        public string FilePath = filePath;
        public Stack<ParsedFile> UpperFiles { get; set; } = upperFiles;
        public SplitData? ImportedFrom = importedFrom;
    }
}
