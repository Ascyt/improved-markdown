using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class ParsedFile(string filePath, string directoryTree, string title, string fullContents, Stack<ParsedFile> upperFiles, SplitData? importedFrom)
    {
        public string FilePath = filePath;
        public string DirectoryTree = directoryTree;
        public string Title = title;
        public string FullContents = fullContents;
        public Stack<ParsedFile> UpperFiles { get; set; } = upperFiles;
        public Stack<ParsedFile> FullStack { get {
                Stack<ParsedFile> s = new Stack<ParsedFile>(UpperFiles);
                s.Push(this);
                return s;
            } }
        public SplitData? ImportedFrom = importedFrom;
    }
}
