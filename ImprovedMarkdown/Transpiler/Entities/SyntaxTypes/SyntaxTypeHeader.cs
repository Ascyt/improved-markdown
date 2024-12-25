using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities.SyntaxTypes
{
    internal class SyntaxTypeHeader(string contents, ParsedFile file, int rowIndex, int colIndex,
        int depth) : SplitData(contents, file, rowIndex, colIndex)
    {
        public int Depth { get; set; } = depth;
    }
}
