using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using ImprovedMarkdown.Transpiler.Entities.Paragraph;
using System.Text;

namespace ImprovedMarkdown.Transpiler
{
    internal static class ParagraphFormatter
    {
        public static List<SplitData> FormatParagraphs(this List<SplitData> data)
        {
            List<SplitData> output = [.. data];

            foreach (SplitData part in output)
            {
                if (part is SyntaxTypeParagraph partTypeParagraph)
                {
                    partTypeParagraph.Contents = partTypeParagraph.Contents.Replace("\r", "");
                    part.Contents = FormatSingleParagraph(partTypeParagraph);
                }
            }

            return output;
        }
        private static string FormatSingleParagraph(SyntaxTypeParagraph data)
        {
            string contents = data.Contents;
            List<ParagraphToken> tokens = ParagraphTokenize(contents);
            int index = 0;
            ParagraphNode? ast = ParseContent(tokens, ref index);
            string html = RenderNodes(ast);
            return html;

            // ParagraphTokenization step: Convert the input string into a list of tokens
            List<ParagraphToken> ParagraphTokenize(string text)
            {
                List<ParagraphToken> tokens = new();
                int i = 0;
                while (i < text.Length)
                {
                    char c = text[i];

                    if (c == '\\')
                    {
                        // Handle escape sequences
                        if (i + 1 < text.Length)
                        {
                            char nextChar = text[i + 1];
                            switch (nextChar)
                            {
                                case 'n':
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.LineBreak, "<br>"));
                                    i += 2;
                                    continue;
                                case 't':
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.Tab, "\t"));
                                    i += 2;
                                    continue;
                                case '\\':
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.Text, "\\"));
                                    i += 2;
                                    continue;
                                default:
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.Text, nextChar.ToString()));
                                    i += 2;
                                    continue;
                            }
                        }
                        else
                        {
                            // Single backslash at the end of text
                            tokens.Add(new ParagraphToken(ParagraphTokenType.Text, "\\"));
                            i++;
                            continue;
                        }
                    }
                    else if (c == '*')
                    {
                        int count = 1;
                        while (i + count < text.Length && text[i + count] == '*')
                            count++;

                        int asterisks = count >= 2 ? 2 : 1;
                        tokens.Add(new ParagraphToken(
                            asterisks == 2 ? ParagraphTokenType.DoubleAsterisk : ParagraphTokenType.Asterisk,
                            new string('*', asterisks)
                        ));
                        i += asterisks;
                        continue;
                    }
                    else if (c == '\r')
                    {
                        // Ignore carriage returns
                        i++;
                        continue;
                    }
                    else if (c == '\n')
                    {
                        // Check for empty lines (double newlines)
                        if (i + 1 < text.Length && text[i + 1] == '\n')
                        {
                            tokens.Add(new ParagraphToken(ParagraphTokenType.EmptyLine, "\n\n"));
                            i += 2;
                        }
                        else
                        {
                            tokens.Add(new ParagraphToken(ParagraphTokenType.NewLine, "\n"));
                            i++;
                        }
                        continue;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        int start = i;
                        while (i < text.Length && char.IsWhiteSpace(text[i]) && text[i] != '\n' && text[i] != '\r')
                            i++;
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Whitespace, text.Substring(start, i - start)));
                        continue;
                    }
                    else
                    {
                        // Regular text
                        int start = i;
                        while (i < text.Length && !"*\\\n\r".Contains(text[i]))
                            i++;
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Text, text.Substring(start, i - start)));
                        continue;
                    }
                }

                return tokens;
            }

            // Parsing step: Build an abstract syntax tree (AST) using recursive descent
            ParagraphNode? ParseContent(List<ParagraphToken> tokens, ref int index, ParagraphTokenType? stopAtParagraphToken = null)
            {
                ParagraphNode contentParagraphNode = new ParagraphNode(ParagraphNodeType.Root);

                while (index < tokens.Count)
                {
                    ParagraphToken token = tokens[index];

                    // If we find the stop token, we need to return
                    if (stopAtParagraphToken != null && token.Type == stopAtParagraphToken)
                    {
                        index++; // Consume the stop token
                        return contentParagraphNode;
                    }

                    if (token.Type == ParagraphTokenType.Asterisk || token.Type == ParagraphTokenType.DoubleAsterisk)
                    {
                        index++;
                        ParagraphNodeType formatType = token.Type == ParagraphTokenType.Asterisk ? ParagraphNodeType.Emphasis : ParagraphNodeType.Strong;
                        ParagraphNode formatParagraphNode = new ParagraphNode(formatType);
                        ParagraphNode? innerContent = ParseContent(tokens, ref index, token.Type);
                        if (innerContent != null)
                        {
                            formatParagraphNode.Children.AddRange(innerContent.Children);
                            contentParagraphNode.Children.Add(formatParagraphNode);
                        }
                        else
                        {
                            // Unmatched formatting marker; treat the marker as text
                            contentParagraphNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, token.Value));
                        }
                    }
                    else if (token.Type == ParagraphTokenType.Text || token.Type == ParagraphTokenType.Whitespace || token.Type == ParagraphTokenType.Tab)
                    {
                        contentParagraphNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, token.Value));
                        index++;
                    }
                    else if (token.Type == ParagraphTokenType.LineBreak || token.Type == ParagraphTokenType.EmptyLine)
                    {
                        contentParagraphNode.Children.Add(new ParagraphNode(ParagraphNodeType.LineBreak, "<br>"));
                        index++;
                    }
                    else if (token.Type == ParagraphTokenType.NewLine)
                    {
                        contentParagraphNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, " "));
                        index++;
                    }
                    else
                    {
                        // Unhandled token types
                        index++;
                    }
                }

                if (stopAtParagraphToken != null)
                {
                    // We expected a stop token but didn't find it
                    // Return null to indicate an unmatched formatting marker
                    return null;
                }

                return contentParagraphNode;
            }

            // Rendering step: Convert the AST into HTML
            string RenderNodes(ParagraphNode? node)
            {
                if (node is null)
                    return string.Empty;

                StringBuilder htmlBuilder = new();
                foreach (var child in node.Children)
                {
                    switch (child.Type)
                    {
                        case ParagraphNodeType.Text:
                            htmlBuilder.Append(child.Value);
                            break;
                        case ParagraphNodeType.Emphasis:
                            htmlBuilder.Append("<em>");
                            htmlBuilder.Append(RenderNodes(child));
                            htmlBuilder.Append("</em>");
                            break;
                        case ParagraphNodeType.Strong:
                            htmlBuilder.Append("<strong>");
                            htmlBuilder.Append(RenderNodes(child));
                            htmlBuilder.Append("</strong>");
                            break;
                        case ParagraphNodeType.LineBreak:
                            htmlBuilder.Append("<br>");
                            break;
                        default:
                            htmlBuilder.Append(RenderNodes(child));
                            break;
                    }
                }
                return htmlBuilder.ToString();
            }
        }
    }
}
