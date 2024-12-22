using ImprovedMarkdown.Transpiler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler
{
    internal static class HtmlInjector
    {
        public static string InjectInto(this HtmlComponents components, string html)
        {
            return html
                .Replace("{{BODY}}", components.Body);
        }
    }
}
