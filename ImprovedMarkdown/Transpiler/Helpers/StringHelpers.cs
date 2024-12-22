using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedMarkdown.Transpiler.Helpers
{
    internal static class StringHelpers
    {
        public static int GetStringBeginCharacterCount(this string str, char chr)
        {
            int i = 0;
            while (i < str.Length && str[i] == chr)
                i++;

            return i;
        }
    }
}
