using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class DirectoryNode(string name, Dictionary<string, string>? files = null, Dictionary<string, DirectoryNode>? directories = null)
    {
        public string Name { get; set; } = name;
        // <name, contents>
        public Dictionary<string, string> Files = files ?? new();
        // <name, dir>
        public Dictionary<string, DirectoryNode> Directories = directories ?? new();
    }
}
