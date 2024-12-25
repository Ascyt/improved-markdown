using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class SyntaxException(Stack<ParsedFile> fileStack, int fromRow, int toRow, int fromCol, int toCol, string message) 
        : Exception(message)
    {
        public Stack<ParsedFile> FileStack { get; set; } = fileStack;
        public int FromRow { get; set; } = fromRow;
        public int ToRow { get; set; } = toRow;
        public int FromCol { get; set; } = fromCol;
        public int ToCol { get; set; } = toCol;

        public void Print()
        {
            throw new NotImplementedException();
        }
    }
}
