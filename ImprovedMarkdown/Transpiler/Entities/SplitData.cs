using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal abstract class SplitData(string contents, ParsedFile file, int rowIndex, int colIndex)
    {
        public string Contents = contents;
        public ParsedFile File = file;
        public int rowIndex = rowIndex;
        public int colIndex = colIndex;
    }
}
