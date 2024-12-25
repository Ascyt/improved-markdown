using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class ParagraphFormatter
    {
        public static List<SplitData> FormatParagraphs(this List<SplitData> data)
        {
            List<SplitData> output = new();

            foreach (SplitData part in data)
            {
                if (part is SyntaxTypeParagraph)
                {

                }
                else
                {
                    output.Add(part);
                }
            }

            throw new NotImplementedException();
        }

        private static List<SplitData> FormatSingleParagraph(SplitData data)
        {
            throw new NotImplementedException();
        }
    }
}
