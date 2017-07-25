using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocketbox
{
    internal interface RbCommand
    {
        string GetResponse(string arguments);
        void Execute(string arguments);
    }

    internal class NullCommand : RbCommand
    {
        internal NullCommand() { }

        public string GetResponse(string arguments)
        {
            return "Unknown command.";
        }

        public void Execute(string arguments)
        {
            // do nothing
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

        public void Execute(string arguments)
        {

        }
    }
}
