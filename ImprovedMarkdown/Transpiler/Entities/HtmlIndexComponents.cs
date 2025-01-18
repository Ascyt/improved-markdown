using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class HtmlIndexComponents(string title, string files, string dictionaries, string parents)
    {
        public string Title { get; } = title;
        public string Files { get; } = files;
        public string Dictionaries { get; } = dictionaries;
        public string Parents { get; } = parents;
    }
}
