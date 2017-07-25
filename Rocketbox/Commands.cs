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
}
