using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Rocketbox
{
    internal interface RbCommand
    {
        string GetResponse(string arguments);
        bool Execute(string arguments);
    }

    internal class NullCommand : RbCommand
    {
        internal NullCommand() { }

        public string GetResponse(string arguments)
        {
            return "Unknown command.";
        }

        public bool Execute(string arguments)
        {
            // do nothing
            return false;
        }
    }

    internal class SearchCommand : RbCommand
    {
        private RbSearchEngine _engine;

        internal SearchCommand(RbSearchEngine engine)
        {
            _engine = engine;
        }

        public string GetResponse(string arguments)
        {
            string response = String.Format("{0}: \"{1}\"", _engine.Name, arguments);
            return response;
        }

        public bool Execute(string arguments)
        {
            arguments = arguments.Replace("#", "%23"); // hashtags

            string url = _engine.SearchUrl + arguments;

            Process.Start(url);

            return true;
        }
    }

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
