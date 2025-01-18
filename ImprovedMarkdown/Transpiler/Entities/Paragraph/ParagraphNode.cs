using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities.Paragraph
{
    public enum ParagraphNodeType
    {
        Root,
        Text,
        Emphasis,
        Strong,
        LineBreak
    }

    public class ParagraphNode
    {
        public ParagraphNodeType Type { get; }
        public string? Value { get; }
        public List<ParagraphNode> Children { get; }

        public ParagraphNode(ParagraphNodeType type, string? value = null)
        {
            Type = type;
            Value = value;
            Children = new List<ParagraphNode>();
        }
    }
}
