using ImprovedMarkdown.Transpiler.Entities;
using ImprovedMarkdown.Transpiler.Helpers;
using NUglify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class HtmlInjector
    {
        private static string GetMinifiedHtml(string html)
        {
            return Uglify.Html(html).Code;
        }

        public static string InjectInto(this HtmlComponents components, string html)
        {
            return GetMinifiedHtml(html
                .Replace("{{SIDEBAR}}", components.Sidebar)
                .Replace("{{NAVBAR}}", components.Navbar)
                .Replace("{{BODY}}", components.Body)
                .Replace("{{TITLE}}", components.Title)
                .Replace("{{PARENTS}}", components.Parents));
        }
        public static string InjectInto(this HtmlIndexComponents components, string html)
        {
            return GetMinifiedHtml(html
                .Replace("{{TITLE}}", components.Title)
                .Replace("{{FILES}}", components.Files)
                .Replace("{{DIRECTORIES}}", components.Dictionaries)
                .Replace("{{PARENTS}}", components.Parents));
        }
    } 
}
