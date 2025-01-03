using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using NUglify.Html;
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
            List<SplitData> output = [..data];

            foreach (SplitData part in output)
            {
                if (part is SyntaxTypeParagraph partTypeParagraph)
                {
                    part.Contents = FormatSingleParagraph(partTypeParagraph);
                }
            }

            return output;
        }

        private static string FormatSingleParagraph(SyntaxTypeParagraph data)
        {
            string contents = data.Contents;

            bool isCurrentlyEscaped = false;
            bool previousIsLinebreak = false;

            StringBuilder output = new();
            StringBuilder currentWhitespaces = new(); // whitespaces before \n should be ignored, otherwise added

            for (int i = 0; i < contents.Length; i++)
            {
                char c = contents[i];

                if (c == '\r')
                    continue; // fuck you

                if (c == '\\')
                {
                    if (isCurrentlyEscaped) // double backslash
                    {
                        isCurrentlyEscaped = false;
                        output.Append('\\');
                        continue;
                    }
                    isCurrentlyEscaped = true;

                    previousIsLinebreak = false;
                    AddWhitespaces();
                    continue;
                }

                if (isCurrentlyEscaped)
                {
                    isCurrentlyEscaped = false;

                    bool escapeDone = false;
                    switch (c)
                    {
                        case 'n':
                            output.Append("<br>");
                            escapeDone = true;  
                            break;
                        case 't':
                            output.Append('\t');
                            escapeDone = true;
                            break;
                        // TODO: Add support for unicode escape characters with \[{hex code}]
                    }
                    if (escapeDone)
                        continue;

                    if (c == '\n') // backslash at the end of line
                    {
                        output.Append("<br>");

                        currentWhitespaces.Clear();
                        continue;
                    }

                    if (!char.IsSymbol(c) && !"*".Contains(c))
                    {
                        ThrowException(i, $"Unknown escape sequence: \\{c}");
                    }

                    output.Append(c);
                    continue;
                }

                if (c == '\n')
                {
                    currentWhitespaces.Clear();

                    if (previousIsLinebreak) // double linebreak
                    {
                        output.Append("<br>");
                    }
                    else // singular linebreak only adds a space
                    {
                        output.Append(" ");
                    }
                    previousIsLinebreak = true;

                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    currentWhitespaces.Append(c);
                    continue;
                }

                previousIsLinebreak = false;
                AddWhitespaces();

                output.Append(c);
            }

            return output.ToString();

            void AddWhitespaces()
            {
                output.Append(currentWhitespaces);
                currentWhitespaces.Clear();
            }

            void ThrowException(int i, string message)
            {
                string beforeContents = data.Contents.Substring(0, i);

                int linebreakCount = beforeContents.Count(c => c == '\n');
                int lastLinebreak = beforeContents.LastIndexOf('\n');
                int row = data.rowIndex + linebreakCount;
                int col = i - lastLinebreak;

                throw new SyntaxException(data.File.FullStack, row, row, col, col + 1,
                    message);
            }
        }
    }
}
