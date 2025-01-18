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
        EmptyLine,
        Asterisk,
        Underscore,
        Tilde,
        Backtick,
        TripleBacktick,
        OpenParen,
        CloseParen,
        OpenBracket,
        CloseBracket,
        Exclamation,
        Dash,
        Plus,
        Number,
        Period,
        GreaterThan,
        VerticalBar, // For tables
        HexUnicode,
        Space,
        CheckboxUnchecked,
        CheckboxChecked,
        Tab,
        LineBreak,
        HorizontalRule,
        DoubleUnderscore,
        DoubleTilde,
        DoubleAsterisk,
        Bullet
    }

    public class ParagraphToken
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
