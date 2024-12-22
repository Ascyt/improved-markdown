using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class HtmlComponents(string body)
    {
        public string Body { get; set; } = body;
    }
}
