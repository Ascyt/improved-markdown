using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities.SyntaxTypes
{
    internal class SyntaxTypeTab(string contents, ParsedFile file, int rowIndex, int colIndex) : SplitData(contents, file, rowIndex, colIndex)
    {
    }
}
