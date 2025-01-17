using PowerArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown
{
    internal class ProgramArgs
    {
        [ArgShortcut("-i")]
        [ArgRequired(PromptIfMissing = true)]
        public string InputDir { get; set; }

        [ArgShortcut("-o")]
        [ArgRequired(PromptIfMissing = true)]
        public string OutputDir { get; set; }
    }
}
