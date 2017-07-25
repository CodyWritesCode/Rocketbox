using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    internal static class ResponseInvoker
    {
        internal static string GetResponse(string command)
        {
            if(command.Trim() == string.Empty)
            {
                return new NullCommand().GetResponse(command);
            }

            string keyword = RbUtility.GetKeyword(command);
            string parameters = RbUtility.StripKeyword(command);

            // first test for search engines
            var matchingSearchEngines = from engine in RbData.SearchEngines
                                        where engine.Keywords.Contains(keyword)
                                        select engine;

            if(matchingSearchEngines.Count() != 0)
            {
                SearchCommand searchCommand = new SearchCommand(matchingSearchEngines.First());
                return searchCommand.GetResponse(parameters);
            }

            return new NullCommand().GetResponse(command);
        }
    }

    internal static class ActionInvoker
    {
        internal static bool Execute(string command)
        {
            if(command.Trim() == string.Empty)
            {
                // temporary
                new NullCommand().Execute(command);
                return false;
            }

            // TODO: move duplicate code
            string keyword = RbUtility.GetKeyword(command);
            string parameters = RbUtility.StripKeyword(command);

            // first test for search engines
            var matchingSearchEngines = from engine in RbData.SearchEngines
                                        where engine.Keywords.Contains(keyword)
                                        select engine;

            if (matchingSearchEngines.Count() != 0)
            {
                SearchCommand searchCommand = new SearchCommand(matchingSearchEngines.First());
                if(searchCommand.Execute(parameters))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
