using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Rocketbox
{
    /// <summary>
    /// Defines the structure of a command to be used with the Invoker.
    /// </summary>
    internal interface RbCommand
    {
        // response string will be displayed in the text below the input box before the user sends the command
        string GetResponse(string arguments);

        // runs the command - the boolean signals to the invoker whether Rocketbox should be closed or not
        bool Execute(string arguments);
    }

    /// <summary>
    /// Dummy command for blank or whitespace-only input.
    /// </summary>
    internal class NullCommand : RbCommand
    {
        internal NullCommand() { }

        public string GetResponse(string arguments)
        {
            return "Invalid command.";
        }

        public bool Execute(string arguments)
        {
            // do nothing
            return false;
        }
    }

    /// <summary>
    /// Command for launching search engine queries.
    /// </summary>
    internal class SearchCommand : RbCommand
    {
        private RbSearchEngine _engine;

        // the search engine is determined by the Invoker and passed here
        internal SearchCommand(RbSearchEngine engine)
        {
            _engine = engine;
        }

        // will print the search engine's full name and the query to be sent
        public string GetResponse(string arguments)
        {
            string response = String.Format("{0}:   {1}", _engine.Name, arguments);
            return response;
        }

        // launches the query through a default browser
        public bool Execute(string arguments)
        {
            arguments = arguments.Replace("#", "%23"); // hashtags

            string url = _engine.SearchUrl + arguments;

            Process.Start(url);

            return true;
        }
    }

    /// <summary>
    /// Command to launch an application as if it were from a normal command line.
    /// </summary>
    internal class ShellCommand : RbCommand
    {
        private string _command;

        internal ShellCommand(string command)
        {
            _command = command;
        }

        // parameter can be null
        public string GetResponse(string arguments)
        {
            return string.Format("Run: {0}", _command);
        }

        // parameter can be null
        public bool Execute(string arguments)
        {
            // the entire command is passed into the constructor
            // we need to differentiate between the process and the arguments because System.Diagnostics said so
            string process = RbUtility.GetKeyword(_command);
            string args = RbUtility.StripKeyword(_command);

            ProcessStartInfo info = new ProcessStartInfo(@process);
            info.Arguments = args;
            try
            {
                Process.Start(info);
            }
            catch(Exception e)
            {
                // if launch fails, just do nothing
                return false;
            }

            return true;
        }
    }
}
