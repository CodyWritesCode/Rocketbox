using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    internal static class RbUtility
    {
        internal static string GetKeyword(string input)
        {
            string keyword = input.Split(' ')[0].ToUpper();

            return keyword;
        }

        internal static string StripKeyword(string input)
        {
            string[] commandParts = input.Split(' ');
            commandParts[0] = string.Empty;
            return string.Join(" ", commandParts).Trim();
        }

        internal static void ThrowConfigError(string information)
        {
            throw new Exception("Malformed configuration file: " + information);
        }
    }
}
