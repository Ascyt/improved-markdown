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
                output.AddRange(SpitSingleFileByParts(splitData));
            }

            return output;
        }

        private static List<SplitData> SpitSingleFileByParts(SplitData data)
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

                    if (arg.Length == 0)
                    {
                        throw new SyntaxException(data.File.FullStack, i, i, headingCount, headingCount,
                            $"Argument expected");
                    }

                    SplitData newSplitData = new SyntaxTypeHeader(arg, data.File, i, line.IndexOf(arg), headingCount);
                    output.Add(newSplitData);

                    continue;
                }

                // Tabs
                if (line.StartsWith("@"))
                {
                    ParagraphFinished();

                    string arg = line.Substring(1).Trim();

                    if (arg.Length == 0)
                    {
                        throw new SyntaxException(data.File.FullStack, i, i, 1, 1,
                            $"Argument expected");
                    }

                    SplitData newSplitData = new SyntaxTypeTab(arg, data.File, i, line.IndexOf(arg));
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

                int row = lineStartCurrent + 1;
                lineStartCurrent = -1;

                string paragraph = current.ToString().Trim();
                if (string.IsNullOrEmpty(paragraph))
                    return;

                SplitData newSplitdata = new SyntaxTypeParagraph(paragraph, data.File, row, 0);
                output.Add(newSplitdata);

                current = new StringBuilder("");
            }
        }
    }
}
