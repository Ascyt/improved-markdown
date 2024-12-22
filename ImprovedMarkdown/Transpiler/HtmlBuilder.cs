using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using ImprovedMarkdown.Transpiler.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class HtmlBuilder
    {
        public static HtmlComponents BuildHtmlComponents(this List<SplitData> data)
        {
            StringBuilder body = new();
            bool inSection = false;
            HashSet<string> sectionIds = new();

            foreach (SplitData part in data)
            {
                if (part.type is SyntaxTypeParagraph partTypeParagraph)
                {
                    StringBuilder paragraph = new($"<p>{part.Contents.Trim()}</p>");
                    body.Append(paragraph);
                }
                if (part.type is SyntaxTypeHeader partTypeHeader)
                {
                    StringBuilder sectionStart = new();
                    string sectionId = part.Contents.FormatStringToId(sectionIds);
                    sectionIds.Add(sectionId);

                    if (inSection)
                        sectionStart.Append("</div>");
                    sectionStart.Append($"<div id=\"{sectionId}\">" +
                        $"<h{partTypeHeader.Depth}>{part.Contents}</h{partTypeHeader.Depth}>");

                    body.Append(sectionStart);
                    inSection = true;
                }
            }

            return new HtmlComponents(body.ToString());
        }
    }
}
