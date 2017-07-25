using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    internal static class Invoker
    {
        private static string _currentText;
        private static RbCommand _currentCmd;

        private static string Keyword
        {
            get
            {
                return RbUtility.GetKeyword(_currentText);
            }
        }

        private static string Parameters
        {
            get
            {
                return RbUtility.StripKeyword(_currentText);
            }
        }

        internal static void Invoke(string command)
        {
            _currentText = command;
            _currentCmd = new ShellCommand(command);

            // first test for search engines
            var matchingSearchEngines = from engine in RbData.SearchEngines
                                        where engine.Keywords.Contains(Keyword)
                                        select engine;

            if (matchingSearchEngines.Count() != 0)
            {
                _currentCmd = new SearchCommand(matchingSearchEngines.First());
            }
        }

        internal static string GetResponse()
        {
            if(_currentText.Trim() == string.Empty)
            {
                return new NullCommand().GetResponse(_currentText);
            }

            return _currentCmd.GetResponse(Parameters);
        }

        internal static bool Execute()
        {
            if(_currentText.Trim() == string.Empty)
            {
                return false;
            }

            return _currentCmd.Execute(Parameters);
        }
    }
}
