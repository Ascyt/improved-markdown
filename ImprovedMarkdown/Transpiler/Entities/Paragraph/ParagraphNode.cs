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
        Strikethrough,
        CodeInline,
        CodeBlock,
        Link,
        Image,
        OrderedList,
        UnorderedList,
        ListItem,
        BlockQuote,
        HorizontalRule,
        LineBreak,
        TaskListItem,
        Table,
        TableRow,
        TableHeader,
        TableCell,
    }

    public class ParagraphNode
    {
        public ParagraphNodeType Type { get; }
        public string? Value { get; set; }
        public List<ParagraphNode> Children { get; }

        public ParagraphNode(ParagraphNodeType type, string? value = null)
        {
            Type = type;
            Value = value;
            Children = new List<ParagraphNode>();
        }

        // Method to render the text content of the node and its children
        public string RenderText()
        {
            if (Type == ParagraphNodeType.Text && Value != null)
            {
                return Value;
            }

            StringBuilder textBuilder = new StringBuilder();
            foreach (var child in Children)
            {
                textBuilder.Append(child.RenderText());
            }
            return textBuilder.ToString();
        }
    }
}
