using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    internal static class RbUtility
    {
        /// <summary>
        /// Gets the keyword from a command string
        /// </summary>
        /// <param name="input">The command string</param>
        /// <returns>The first word</returns>
        internal static string GetKeyword(string input)
        {
            string keyword = input.Split(' ')[0].ToUpper();

            return keyword;
        }

        /// <summary>
        /// Removes the keyword from a command string
        /// </summary>
        /// <param name="input">The command string</param>
        /// <returns>The string with the first word removed</returns>
        internal static string StripKeyword(string input)
        {
            string[] commandParts = input.Split(' ');
            commandParts[0] = string.Empty;
            return string.Join(" ", commandParts).Trim();
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="information"></param>
        internal static void ThrowConfigError(string information)
        {
            throw new Exception("Malformed configuration file: " + information);
        }

        /// <summary>
        /// Checks if an icon exists in the icons directory
        /// </summary>
        /// <param name="icon">The icon file name to be checked</param>
        /// <returns></returns>
        internal static bool IconExists(string icon)
        {
            if(File.Exists(Environment.CurrentDirectory + "\\icons\\" + icon))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
