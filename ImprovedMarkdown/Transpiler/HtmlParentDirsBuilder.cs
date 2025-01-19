using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class HtmlParentDirsBuilder
    {
        /// <param name="parents">(dir, title)</param>
        /// <returns></returns>
        public static string BuildHtmlParentDirs(this (string, string)[] parents, string outputRootDirLocation)
        {
            if (parents == null || parents.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder htmlBuilder = new StringBuilder();
            string currentPath = string.Empty;

            for (int i = 0; i < parents.Length; i++)
            {
                var (dir, title) = parents[i];

                if (i > 0)
                {
                    htmlBuilder.Append(" / ");
                }

                if (title != dir && !string.IsNullOrWhiteSpace(dir))
                {
                    currentPath = string.IsNullOrEmpty(currentPath) ? dir : $"{currentPath}/{dir}";

                    string directory = new FileInfo(Path.Join(outputRootDirLocation, currentPath)).FullName;

                    string upperPath = Path.Join(directory, "index.html");

                    htmlBuilder.Append($"<a href=\"file://{upperPath}\">{title}</a>");
                }
                else
                {
                    htmlBuilder.Append($"<b>{title}</b>");
                }
            }

            return htmlBuilder.ToString();
        }
    }
}
