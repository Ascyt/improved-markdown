using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using Markdig;
using Markdig.SyntaxHighlighting;
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
            return Markdown.ToHtml(data.Contents, new MarkdownPipelineBuilder()
                .UsePipeTables()
                .UseEmphasisExtras()
                .UseGridTables()
                .UseGenericAttributes()
                .UseDefinitionLists()
                .UseFootnotes()
                .UseAutoLinks()
                .UseTaskLists()
                .UseListExtras()
                .UseMediaLinks()
                .UseAbbreviations()
                .UseFigures()
                .UseFooters()
                .UseCustomContainers()
                .UseMathematics()
                .UseDiagrams()
                .UseAdvancedExtensions()
                .UseSyntaxHighlighting()
                .Build());
        }
    }
}
