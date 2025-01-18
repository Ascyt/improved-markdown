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
        }

        private static List<ParagraphToken> ParagraphTokenize(string text)
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
                        if (nextChar == 'x' && i + 2 < text.Length && text[i + 2] == '[')
                        {
                            // Hex Unicode sequence \x[code]
                            int startIndex = i + 3;
                            int endIndex = text.IndexOf(']', startIndex);
                            if (endIndex != -1)
                            {
                                string code = text.Substring(startIndex, endIndex - startIndex);
                                tokens.Add(new ParagraphToken(ParagraphTokenType.HexUnicode, code));
                                i = endIndex + 1;
                                continue;
                            }
                            else
                            {
                                // Invalid sequence, treat as text
                                tokens.Add(new ParagraphToken(ParagraphTokenType.Text, "\\x["));
                                i += 3;
                                continue;
                            }
                        }
                        else
                        {
                            // Handle other escape sequences
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
                else if (c == '`')
                {
                    // Handle backticks for code inline and code block
                    int count = 0;
                    int start = i;
                    while (i < text.Length && text[i] == '`')
                    {
                        count++;
                        i++;
                    }

                    string backticks = text.Substring(start, count);

                    if (count >= 3)
                    {
                        // Triple backtick (code block)
                        tokens.Add(new ParagraphToken(ParagraphTokenType.TripleBacktick, backticks));
                    }
                    else
                    {
                        // Inline code
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Backtick, backticks));
                    }
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
                    // Handle newlines
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
                    if (c == ' ')
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Space, " "));
                        i++;
                    }
                    else
                    {
                        int start = i;
                        while (i < text.Length && char.IsWhiteSpace(text[i]) && text[i] != '\n' && text[i] != '\r')
                            i++;
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Whitespace, text.Substring(start, i - start)));
                    }
                    continue;
                }
                else if (c == '>')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.GreaterThan, ">"));
                    i++;
                    continue;
                }
                else if (c == '-' || c == '+')
                {
                    // Could be unordered list, task list, or horizontal rule
                    int start = i;
                    char bulletChar = c;
                    int count = 0;
                    while (i < text.Length && text[i] == bulletChar)
                    {
                        count++;
                        i++;
                    }
                    if (count >= 3 && (i == text.Length || text[i] == '\n'))
                    {
                        // Horizontal rule
                        tokens.Add(new ParagraphToken(ParagraphTokenType.HorizontalRule, text.Substring(start, count)));
                        continue;
                    }
                    else if (bulletChar == '-' || bulletChar == '+')
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Dash, bulletChar.ToString()));
                        // Check for task list
                        if (i + 2 < text.Length && text[i] == ' ' && text[i + 1] == '[')
                        {
                            i++; // Skip space
                            i++; // Skip '['
                            if (i < text.Length)
                            {
                                if (text[i] == ' ')
                                {
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.CheckboxUnchecked, " "));
                                    i++;
                                }
                                else if (text[i] == 'x' || text[i] == 'X')
                                {
                                    tokens.Add(new ParagraphToken(ParagraphTokenType.CheckboxChecked, text[i].ToString()));
                                    i++;
                                }
                                if (i < text.Length && text[i] == ']')
                                {
                                    i++; // Skip ']'
                                }
                            }
                        }
                        continue;
                    }
                }
                else if (c == '*')
                {
                    // Handle asterisks for emphasis and strong
                    int count = 0;
                    int start = i;
                    while (i < text.Length && text[i] == '*')
                    {
                        count++;
                        i++;
                    }
                    string stars = text.Substring(start, count);

                    if (count >= 2)
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Asterisk, stars));
                    }
                    else
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Asterisk, stars));
                    }
                    continue;
                }
                else if (c == '[')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.OpenBracket, "["));
                    i++;
                    continue;
                }
                else if (c == ']')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.CloseBracket, "]"));
                    i++;
                    continue;
                }
                else if (c == '(')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.OpenParen, "("));
                    i++;
                    continue;
                }
                else if (c == ')')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.CloseParen, ")"));
                    i++;
                    continue;
                }
                else if (c == '!')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.Exclamation, "!"));
                    i++;
                    continue;
                }
                else if (c == '#')
                {
                    // We can handle headers here if needed
                    // For now, treat as text
                    tokens.Add(new ParagraphToken(ParagraphTokenType.Text, c.ToString()));
                    i++;
                    continue;
                }
                else if (c == '|')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.VerticalBar, "|"));
                    i++;
                    continue;
                }
                else if (char.IsDigit(c))
                {
                    int start = i;
                    while (i < text.Length && char.IsDigit(text[i]))
                        i++;
                    if (i < text.Length && text[i] == '.' && (start == 0 || text[start - 1] == '\n' || tokens.Count == 0 || tokens[^1].Type == ParagraphTokenType.NewLine))
                    {
                        string number = text.Substring(start, i - start);
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Number, number));
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Period, "."));
                        i++; // Skip the dot
                             // Skip optional space
                        if (i < text.Length && text[i] == ' ')
                            i++;
                        continue;
                    }
                    else
                    {
                        // Not a list item, just text
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Text, text.Substring(start, i - start)));
                        continue;
                    }
                }
                else if (c == '\t')
                {
                    tokens.Add(new ParagraphToken(ParagraphTokenType.Tab, "\t"));
                    i++;
                    continue;
                }
                else if (c == '_')
                {
                    // Handle underscores for emphasis
                    tokens.Add(new ParagraphToken(ParagraphTokenType.Underscore, "_"));
                    i++;
                    continue;
                }
                else if (c == '~')
                {
                    // Handle tildes for strikethrough
                    int count = 0;
                    int start = i;
                    while (i < text.Length && text[i] == '~')
                    {
                        count++;
                        i++;
                    }
                    string tildes = text.Substring(start, count);

                    if (count >= 2)
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Tilde, tildes));
                    }
                    else
                    {
                        tokens.Add(new ParagraphToken(ParagraphTokenType.Tilde, tildes));
                    }
                    continue;
                }
                else
                {
                    // Regular text
                    int start = i;
                    while (i < text.Length && !"*\\`~_[]()!-+>\n\r|".Contains(text[i]))
                        i++;
                    tokens.Add(new ParagraphToken(ParagraphTokenType.Text, text.Substring(start, i - start)));
                    continue;
                }
            }
            return tokens;
        }

        private static ParagraphNode? ParseContent(List<ParagraphToken> tokens, ref int index, ParagraphTokenType? stopAtParagraphToken = null)
        {
            ParagraphNode contentNode = new ParagraphNode(ParagraphNodeType.Root);

            while (index < tokens.Count)
            {
                ParagraphToken token = tokens[index];

                // If we find the stop token, we need to return
                if (stopAtParagraphToken != null && token.Type == stopAtParagraphToken)
                {
                    index++; // Consume the stop token
                    return contentNode;
                }

                if (token.Type == ParagraphTokenType.HexUnicode)
                {
                    // Convert hex code to Unicode character
                    if (int.TryParse(token.Value, System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                    {
                        string unicodeChar = char.ConvertFromUtf32(codePoint);
                        contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, unicodeChar));
                    }
                    else
                    {
                        // Invalid code, treat as text
                        contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, $"\\x[{token.Value}]"));
                    }
                    index++;
                }
                else if (token.Type == ParagraphTokenType.Backtick)
                {
                    // Inline code
                    index++;
                    ParagraphNode codeNode = new ParagraphNode(ParagraphNodeType.CodeInline);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.Backtick);
                    if (innerContent != null)
                    {
                        codeNode.Children.AddRange(innerContent.Children);
                        contentNode.Children.Add(codeNode);
                    }
                    else
                    {
                        // Unmatched backtick, treat as text
                        contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, "`"));
                    }
                }
                else if (token.Type == ParagraphTokenType.TripleBacktick)
                {
                    // Code block
                    index++;
                    ParagraphNode codeBlockNode = new ParagraphNode(ParagraphNodeType.CodeBlock);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.TripleBacktick);
                    if (innerContent != null)
                    {
                        codeBlockNode.Children.AddRange(innerContent.Children);
                        contentNode.Children.Add(codeBlockNode);
                    }
                    else
                    {
                        // Unmatched triple backticks, treat as text
                        contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, "```"));
                    }
                }
                else if (token.Type == ParagraphTokenType.Exclamation && index + 1 < tokens.Count && tokens[index + 1].Type == ParagraphTokenType.OpenBracket)
                {
                    // Image
                    index += 2; // Consume '!' and '['
                    ParagraphNode altTextNode = ParseContent(tokens, ref index, ParagraphTokenType.CloseBracket);
                    if (index < tokens.Count && tokens[index].Type == ParagraphTokenType.OpenParen)
                    {
                        index++; // Consume '('
                        ParagraphNode urlNode = ParseContent(tokens, ref index, ParagraphTokenType.CloseParen);
                        ParagraphNode imageNode = new ParagraphNode(ParagraphNodeType.Image);
                        imageNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, altTextNode?.RenderText()));
                        imageNode.Value = urlNode?.RenderText();
                        contentNode.Children.Add(imageNode);
                    }
                }
                else if (token.Type == ParagraphTokenType.OpenBracket)
                {
                    // Link
                    index++; // Consume '['
                    ParagraphNode linkTextNode = ParseContent(tokens, ref index, ParagraphTokenType.CloseBracket);
                    if (index < tokens.Count && tokens[index].Type == ParagraphTokenType.OpenParen)
                    {
                        index++; // Consume '('
                        ParagraphNode urlNode = ParseContent(tokens, ref index, ParagraphTokenType.CloseParen);
                        ParagraphNode linkNode = new ParagraphNode(ParagraphNodeType.Link);
                        linkNode.Children.AddRange(linkTextNode.Children);
                        linkNode.Value = urlNode?.RenderText();
                        contentNode.Children.Add(linkNode);
                    }
                }
                else if (token.Type == ParagraphTokenType.Asterisk || token.Type == ParagraphTokenType.Underscore)
                {
                    // Emphasis
                    index++;
                    ParagraphNode emphasisNode = new ParagraphNode(ParagraphNodeType.Emphasis);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, token.Type);
                    if (innerContent != null)
                    {
                        emphasisNode.Children.AddRange(innerContent.Children);
                        contentNode.Children.Add(emphasisNode);
                    }
                }
                else if (token.Type == ParagraphTokenType.DoubleAsterisk || token.Type == ParagraphTokenType.DoubleUnderscore)
                {
                    // Strong emphasis
                    index++;
                    ParagraphNode strongNode = new ParagraphNode(ParagraphNodeType.Strong);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, token.Type);
                    if (innerContent != null)
                    {
                        strongNode.Children.AddRange(innerContent.Children);
                        contentNode.Children.Add(strongNode);
                    }
                }
                else if (token.Type == ParagraphTokenType.DoubleTilde)
                {
                    // Strikethrough
                    index++;
                    ParagraphNode strikeNode = new ParagraphNode(ParagraphNodeType.Strikethrough);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, token.Type);
                    if (innerContent != null)
                    {
                        strikeNode.Children.AddRange(innerContent.Children);
                        contentNode.Children.Add(strikeNode);
                    }
                }
                else if (token.Type == ParagraphTokenType.GreaterThan)
                {
                    // Blockquote
                    index++;
                    ParagraphNode quoteNode = new ParagraphNode(ParagraphNodeType.BlockQuote);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.NewLine);
                    if (innerContent != null)
                    {
                        quoteNode.Children.AddRange(innerContent.Children);
                    }
                    contentNode.Children.Add(quoteNode);
                }
                else if (token.Type == ParagraphTokenType.HorizontalRule)
                {
                    // Horizontal rule
                    index++;
                    contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.HorizontalRule));
                }
                else if (token.Type == ParagraphTokenType.Dash || token.Type == ParagraphTokenType.Bullet)
                {
                    // Unordered list
                    index++;
                    ParagraphNode listNode = new ParagraphNode(ParagraphNodeType.UnorderedList);
                    ParagraphNode listItemNode = new ParagraphNode(ParagraphNodeType.ListItem);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.NewLine);
                    if (innerContent != null)
                    {
                        listItemNode.Children.AddRange(innerContent.Children);
                        listNode.Children.Add(listItemNode);
                    }
                    contentNode.Children.Add(listNode);
                }
                else if (token.Type == ParagraphTokenType.Number && index + 1 < tokens.Count && tokens[index + 1].Type == ParagraphTokenType.Period)
                {
                    // Ordered list
                    index += 2; // Consume number and period
                    ParagraphNode listNode = new ParagraphNode(ParagraphNodeType.OrderedList);
                    ParagraphNode listItemNode = new ParagraphNode(ParagraphNodeType.ListItem);
                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.NewLine);
                    if (innerContent != null)
                    {
                        listItemNode.Children.AddRange(innerContent.Children);
                        listNode.Children.Add(listItemNode);
                    }
                    contentNode.Children.Add(listNode);
                }
                else if (token.Type == ParagraphTokenType.Dash && index + 2 < tokens.Count && tokens[index + 1].Type == ParagraphTokenType.Whitespace && tokens[index + 2].Type == ParagraphTokenType.OpenBracket)
                {
                    // Task list item
                    index += 3; // Consume '-', space, '['
                    ParagraphNode taskItemNode = new ParagraphNode(ParagraphNodeType.TaskListItem);

                    if (index < tokens.Count)
                    {
                        if (tokens[index].Type == ParagraphTokenType.CheckboxChecked)
                        {
                            taskItemNode.Value = "checked";
                            index++;
                        }
                        else if (tokens[index].Type == ParagraphTokenType.CheckboxUnchecked)
                        {
                            taskItemNode.Value = "unchecked";
                            index++;
                        }
                    }

                    if (index < tokens.Count && tokens[index].Type == ParagraphTokenType.CloseBracket)
                        index++; // Consume ']'

                    ParagraphNode? innerContent = ParseContent(tokens, ref index, ParagraphTokenType.NewLine);
                    if (innerContent != null)
                    {
                        taskItemNode.Children.AddRange(innerContent.Children);
                    }
                    contentNode.Children.Add(taskItemNode);
                }
                else if (token.Type == ParagraphTokenType.VerticalBar)
                {
                    // Table
                    ParagraphNode tableNode = new ParagraphNode(ParagraphNodeType.Table);
                    while (index < tokens.Count && tokens[index].Type != ParagraphTokenType.EmptyLine)
                    {
                        ParagraphNode rowNode = new ParagraphNode(ParagraphNodeType.TableRow);
                        while (index < tokens.Count && tokens[index].Type != ParagraphTokenType.NewLine)
                        {
                            if (tokens[index].Type == ParagraphTokenType.VerticalBar)
                            {
                                index++; // Consume '|'
                                ParagraphNode cellNode = new ParagraphNode(ParagraphNodeType.TableCell);
                                ParagraphNode? cellContent = ParseContent(tokens, ref index, ParagraphTokenType.VerticalBar);
                                if (cellContent != null)
                                {
                                    cellNode.Children.AddRange(cellContent.Children);
                                }
                                rowNode.Children.Add(cellNode);
                            }
                            else
                            {
                                index++; // Skip other tokens
                            }
                        }
                        index++; // Consume NewLine
                        tableNode.Children.Add(rowNode);
                    }
                    contentNode.Children.Add(tableNode);
                }
                else if (token.Type == ParagraphTokenType.Text || token.Type == ParagraphTokenType.Whitespace || token.Type == ParagraphTokenType.Tab)
                {
                    contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, token.Value));
                    index++;
                }
                else if (token.Type == ParagraphTokenType.LineBreak || token.Type == ParagraphTokenType.EmptyLine)
                {
                    contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.LineBreak, "<br>"));
                    index++;
                }
                else if (token.Type == ParagraphTokenType.NewLine)
                {
                    contentNode.Children.Add(new ParagraphNode(ParagraphNodeType.Text, " "));
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

            return contentNode;
        }

        private static string RenderNodes(ParagraphNode? node)
        {
            if (node is null)
                return string.Empty;

            StringBuilder htmlBuilder = new();
            foreach (var child in node.Children)
            {
                switch (child.Type)
                {
                    case ParagraphNodeType.Text:
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
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
                    case ParagraphNodeType.Strikethrough:
                        htmlBuilder.Append("<del>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</del>");
                        break;
                    case ParagraphNodeType.CodeInline:
                        htmlBuilder.Append("<code>");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                        htmlBuilder.Append("</code>");
                        break;
                    case ParagraphNodeType.CodeBlock:
                        htmlBuilder.Append("<pre><code");
                        if (!string.IsNullOrEmpty(child.Value))
                        {
                            htmlBuilder.Append(" class=\"language-");
                            htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                            htmlBuilder.Append("\"");
                        }
                        htmlBuilder.Append(">");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                        htmlBuilder.Append("</code></pre>");
                        break;
                    case ParagraphNodeType.Link:
                        htmlBuilder.Append("<a href=\"");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                        htmlBuilder.Append("\" target=\"_blank\">");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</a>");
                        break;
                    case ParagraphNodeType.Image:
                        htmlBuilder.Append("<a href=\"");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                        htmlBuilder.Append("\" target=\"_blank\">");
                        htmlBuilder.Append("<img src=\"");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Value));
                        htmlBuilder.Append("\" alt=\"");
                        htmlBuilder.Append(System.Net.WebUtility.HtmlEncode(child.Children.FirstOrDefault()?.Value));
                        htmlBuilder.Append("\">");
                        htmlBuilder.Append("</a>");
                        break;
                    case ParagraphNodeType.UnorderedList:
                        htmlBuilder.Append("<ul>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</ul>");
                        break;
                    case ParagraphNodeType.OrderedList:
                        htmlBuilder.Append("<ol>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</ol>");
                        break;
                    case ParagraphNodeType.ListItem:
                        htmlBuilder.Append("<li>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</li>");
                        break;
                    case ParagraphNodeType.TaskListItem:
                        htmlBuilder.Append("<li>");
                        htmlBuilder.Append("<input type=\"checkbox\" disabled");
                        if (child.Value == "checked")
                            htmlBuilder.Append(" checked");
                        htmlBuilder.Append(">");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</li>");
                        break;
                    case ParagraphNodeType.BlockQuote:
                        htmlBuilder.Append("<blockquote>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</blockquote>");
                        break;
                    case ParagraphNodeType.HorizontalRule:
                        htmlBuilder.Append("<hr>");
                        break;
                    case ParagraphNodeType.Table:
                        htmlBuilder.Append("<table>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</table>");
                        break;
                    case ParagraphNodeType.TableRow:
                        htmlBuilder.Append("<tr>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</tr>");
                        break;
                    case ParagraphNodeType.TableHeader:
                        htmlBuilder.Append("<th>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</th>");
                        break;
                    case ParagraphNodeType.TableCell:
                        htmlBuilder.Append("<td>");
                        htmlBuilder.Append(RenderNodes(child));
                        htmlBuilder.Append("</td>");
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
