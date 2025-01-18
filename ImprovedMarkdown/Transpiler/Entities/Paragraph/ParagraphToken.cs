using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities.Paragraph
{ 
    public enum ParagraphTokenType
    {
        Text,
        Whitespace,
        NewLine,
        LineBreak,
        Tab,
        EmptyLine,
        Asterisk,
        DoubleAsterisk
    }

    class ParagraphToken
    {
        public ParagraphTokenType Type { get; }
        public string Value { get; }

        public ParagraphToken(ParagraphTokenType type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}
