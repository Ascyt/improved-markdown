using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities.SyntaxTypes
{
    internal class SyntaxTypeHeader(int depth) : SyntaxType
    {
        public int Depth { get; set; } = depth;
    }
}
