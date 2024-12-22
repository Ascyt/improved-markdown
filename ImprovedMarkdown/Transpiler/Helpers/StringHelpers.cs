using NUglify.Html;
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

        public static string FormatStringToId(this string str, HashSet<string> existingIds)
        {
            string id = new string(str
                .Normalize(NormalizationForm.FormD)
                .ToLower()
                .Replace(" ", "-")
                .Where(c => c.IsAlphaNumeric() || c == '-' || c == '_')
                .ToArray());

            string idNonRepeating = id;
            int i = 2;
            while (existingIds.Contains(idNonRepeating))
            {
                idNonRepeating = $"{id}-{i}";
                i++;
            }

            return idNonRepeating;
        }
    }
}