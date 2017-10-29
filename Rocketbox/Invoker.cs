using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    /// <summary>
    /// Handles the determination of what actions to be taken with a user command
    /// </summary>
    internal static class Invoker
    {
        private static string _currentText;
        private static RbCommand _currentCmd;

        // easy way to access the keyword of the current command
        private static string Keyword
        {
            get
            {
                return RbUtility.GetKeyword(_currentText);
            }
        }

        // easy way to access the parameters of the current command
        private static string Parameters
        {
            get
            {
                return RbUtility.StripKeyword(_currentText);
            }
        }

        // should be ran on every update before responding/executing
        internal static void Invoke(string command)
        {
            // default to a shell command
            _currentCmd = new ShellCommand(command);
            _currentText = command;

            // first, test for search engines
            var matchingSearchEngines = from engine in RbData.SearchEngines
                                        where engine.Aliases.Contains(Keyword)
                                        select engine;

            // if a search engine is found, change the command
            if (matchingSearchEngines.Count() != 0)
            {
                _currentCmd = new SearchCommand(matchingSearchEngines.First());
            }

            // run through generic commands
            switch(Keyword)
            {
                case "CONVERT":
                case "CON":
                case "CV":
                case "C":
                    _currentCmd = new UnitConversionCommand();
                    break;
                case "TRANSLATE":
                case "TRANS":
                case "TR":
                    _currentCmd = new TranslateCommand();
                    break;
                case "=":
                    _currentCmd = new CalculatorCommand();
                    break;
                case "TIME":
                case "T":
                    _currentCmd = new TimeCommand();
                    break;
                case "T+":
                    _currentCmd = new TimeDiffCommand(RbTimeDiffMode.Add);
                    break;
                case "T-":
                    _currentCmd = new TimeDiffCommand(RbTimeDiffMode.Subtract);
                    break;
            }
        }

        // retrieves the string to be indicated below the text box before a command is sent
        internal static string GetResponse()
        {
            if(_currentText.Trim() == string.Empty)
            {
                return new NullCommand().GetResponse(_currentText);
            }

            return _currentCmd.GetResponse(Parameters);
        }

        // runs the command
        internal static bool Execute()
        {
            if(_currentText.Trim() == string.Empty)
            {
                return false;
            }

            return _currentCmd.Execute(Parameters);
        }

        // sets an icon, if any
        internal static string GetIcon()
        {
            if(_currentText.Trim() == string.Empty)
            {
                return new NullCommand().GetIcon();
            }

            return _currentCmd.GetIcon();
        }
    }
}
