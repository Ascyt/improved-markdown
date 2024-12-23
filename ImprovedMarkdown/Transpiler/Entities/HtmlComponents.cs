﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Entities
{
    internal class HtmlComponents(string sidebar, string navbar, string initialTabId, string body)
    {
        public string Sidebar { get; set; } = sidebar;
        public string Navbar { get; set; } = navbar;
        public string InitialTabId { get; set; } = initialTabId;
        public string Body { get; set; } = body;
    }
}
