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
        public string InputFile { get; set; }

        [ArgShortcut("-o")]
        [ArgRequired(PromptIfMissing = true)]
        public string OutputFile { get; set; }
    }
}
