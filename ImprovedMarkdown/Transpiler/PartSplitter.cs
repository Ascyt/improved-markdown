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
    internal static class PartSplitter
    {
        public static List<SplitData> SplitFilesByParts(this List<SplitData> data)
        {
            List<SplitData> output = new();

            foreach (SplitData splitData in data)
            {
                output.AddRange(SpitSingleFileByPart(splitData));
            }

            return output;
        }

        private static List<SplitData> SpitSingleFileByPart(SplitData data)
        {
            List<SplitData> output = new();

            string[] lines = data.Contents.Split('\n');
            StringBuilder current = new("");
            int lineStartCurrent = -1;
    
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Headers
                if (line.StartsWith("#"))
                {
                    ParagraphFinished();

                    int headingCount = line.GetStringBeginCharacterCount('#');
                    const int MAX_DEPTH = 6;
                    if (headingCount > MAX_DEPTH)
                    {
                        throw new SyntaxException(data.File.FullStack, i, i, MAX_DEPTH, headingCount - 1, 
                            $"Headings cannot have a depth greater than {MAX_DEPTH}.");
                    }

                    string arg = line.Substring(headingCount).Trim();

                    SplitData newSplitData = new(arg, data.File, i, line.IndexOf(arg), new SyntaxTypeHeader(headingCount));
                    output.Add(newSplitData);

                    continue;
                }


                if (lineStartCurrent == -1)
                {
                    lineStartCurrent = i;
                }
                
                current.AppendLine(line);
            }

            ParagraphFinished();

            return output;

            void ParagraphFinished()
            {
                if (lineStartCurrent == -1)
                    return;

                SplitData newSplitdata = new(current.ToString(), data.File, lineStartCurrent, 0, new SyntaxTypeParagraph());
                output.Add(newSplitdata);

                lineStartCurrent = -1;
                current = new StringBuilder("");
            }
        }
    }
}
