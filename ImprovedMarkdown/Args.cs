﻿using PowerArgs;
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
        [ArgDescription("Input directory")]
        public string InputDir { get; set; }

        [ArgShortcut("-o")]
        [ArgRequired(PromptIfMissing = true)]
        [ArgDescription("Output directory")]
        public string OutputDir { get; set; }

        [ArgShortcut("--server")]
        [ArgDescription("Server build, uses HTTP paths instead of local filesystem paths")]
        public bool ServerBuild { get; set; }
    }
}
