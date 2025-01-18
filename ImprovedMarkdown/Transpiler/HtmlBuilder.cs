using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Entities.SyntaxTypes;
using ImprovedMarkdown.Transpiler.Helpers;
using System.Text;

namespace ImprovedMarkdown.Transpiler
{
    internal static class HtmlBuilder
    {
        public static HtmlComponents BuildHtmlComponents(this List<SplitData> data, string outputRootDirLocation)
        {
            StringBuilder body = new();
            StringBuilder sidebar = new();
            StringBuilder navbar = new();

            bool inSection = false;
            bool inTab = false;
            string? inTabId = null;
            HashSet<string> sectionIds = new();
            HashSet<string> tabIds = new();

            bool tabsExist = data.Any(d => d is SyntaxTypeTab);

            if (!tabsExist)
                inTabId = "INDEX";

            foreach (SplitData part in data)
            {
                if (part is SyntaxTypeParagraph partTypeParagraph)
                {
                    StringBuilder paragraph = new($"<p>{part.Contents.Trim()}</p>");
                    body.Append(paragraph);
                }
                if (part is SyntaxTypeHeader partTypeHeader)
                {
                    if (partTypeHeader.Depth == 1)
                    {
                        StringBuilder sectionStart = new();

                        if (inSection)
                        {
                            sectionStart.Append("</div>");
                        }

                        string sectionId = part.Contents.FormatStringToId(sectionIds);
                        sectionIds.Add(sectionId);

                        sectionStart.Append($"<div class=\"section\" id=\"SECTION_{sectionId}\">");

                        if (inTabId is null)
                        {
                            throw new SyntaxException(part.File.FullStack, part.rowIndex, part.rowIndex, 0, part.Contents.Length - 1,
                                "Tabs are used in current document; cannot define section outside of tab.");
                        }

                        sidebar.Append($"<a id=\"SECTION-BTN_{sectionId}\" href=\"#SECTION_{sectionId}\" onclick=\"onSectionClick('{sectionId}')\">{part.Contents}</a>");

                        inSection = true;

                        body.Append(sectionStart);
                    }
                    body.Append($"<h{partTypeHeader.Depth}>{part.Contents}</h{partTypeHeader.Depth}>");
                }
                if (part is SyntaxTypeTab partTypeTab)
                {
                    StringBuilder tabStart = new();
                    string tabId = part.Contents.FormatStringToId(tabIds);
                    tabIds.Add(tabId);
                    if (inSection)
                    {
                        body.Append("</div>");
                        inSection = false;
                    }

                    if (inTab)
                    {
                        tabStart.Append("</div>");
                        sidebar.Append("</div>");
                    }
                    tabStart.Append($"<div class=\"tab\" id=\"TAB_{tabId}\">");
                    navbar.Append($"<button class=\"tab-button\" id=\"TAB-BTN_{tabId}\" onclick=\"onTabClick('{tabId}')\">{part.Contents}</button>");

                    sidebar.Append($"<div class=\"tab-sidebar\" id=\"TAB-SIDEBAR_{tabId}\">");

                    body.Append(tabStart);
                    inTab = true;
                    inTabId = tabId;
                    inSection = false;
                }
            }

            if (inSection)
            {
                body.Append("</div>");
            }

            if (inTab)
            {
                body.Append("</div>");
                sidebar.Append("</div>");
            }

            // Build the parent directories HTML
            var firstFile = data.FirstOrDefault()?.File;
            var parentDirs = new List<(string, string)>() { ("", "root") };

            if (firstFile != null)
            {
                foreach (string dir in firstFile.DirectoryTree.Split('/').Skip(1))
                {
                    parentDirs.Add((dir, dir));
                }
            }

            return new HtmlComponents(
                sidebar: sidebar.ToString(),
                navbar: navbar.ToString(),
                initialTabId: inTabId ?? "",
                body: body.ToString(),
                title: firstFile?.Title ?? "Unnamed Document",
                parents: parentDirs.ToArray().BuildHtmlParentDirs(outputRootDirLocation));
        }
    }
}
