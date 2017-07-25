using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Rocketbox
{
    internal static class RbData
    {
        // Master list of search engines
        internal static List<RbSearchEngine> SearchEngines;

        private static bool isLoaded = false;

        // Must be called by app before doing anything
        internal static void LoadData()
        {
            if(!isLoaded)
            {
                // load search engines from config
                StreamReader reader = new StreamReader("SearchEngines.cfg");
                string fileContents = reader.ReadToEnd();
                reader.Close();

                string[] segments = fileContents.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                SearchEngines = new List<RbSearchEngine>();

                /*  Config format:
                 *      Google                            Name of engine
                 *      https://www.google.ca/search?q=   URL prefix to append parameters to
                 *      GOOGLE                            Keyword
                 *      GOOG                              Keyword
                 *      GL                                Keyword
                 *      ;;                                Delimeter
                 * */

                foreach(string segment in segments)
                {
                    string[] lines = segment.Trim().Split('\n');
                    string[] keywords = lines.Skip(2).ToArray();
                    try
                    {
                        SearchEngines.Add(new RbSearchEngine(lines[0], lines[1], keywords));
                    }
                    catch(Exception e)
                    {
                        throw new Exception("Malformed configuration file.");
                    }
                }


                isLoaded = true;
            }
        }
    }

    internal struct RbSearchEngine
    {
        internal string Name { get; private set; }
        internal string SearchUrl { get; private set; }
        internal string[] Keywords { get; private set; }
       
        internal RbSearchEngine(string name, string searchUrl, params string[] keywords)
        {
            Name = name;
            SearchUrl = searchUrl;

            Keywords = keywords;
        }
    }
}
