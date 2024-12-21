using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal class SplitData(string contents, ParsedFile file, int rowIndex, int colIndex)
    {
        string Contents = contents;
        ParsedFile File = file;
        int rowIndex = rowIndex;
        int colIndex = colIndex;
    }
}
