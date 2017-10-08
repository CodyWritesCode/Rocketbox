using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NCalc;

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

            string url = _engine.Url_Prefix + arguments;

            Process.Start(url);

            return true;
        }
    }

    internal class UnitConversionCommand : RbCommand
    {
        private RbConversionUnit _convertFrom;
        private RbConversionUnit _convertTo;

        private decimal _valueFrom;

        internal UnitConversionCommand() { }

        public string GetResponse(string arguments)
        {
            bool isError = false;

            string[] parts = arguments.Split(' ');

            if(parts.Length < 3)
            {
                return "Unit conversion:   Not enough parameters.";
            }

            /// first and second part = number/unit
            if(!decimal.TryParse(parts[0], out _valueFrom))
            {
                isError = true;
            }

            _convertFrom = RbData.GetConversionUnit(parts[1]);

            // last part = output unit
            // (can use "to" or other combos in the middle)
            _convertTo = RbData.GetConversionUnit(parts.Last());


            if(_convertFrom.GetUnitType() == RbUnitType.Null || _convertTo.GetUnitType() == RbUnitType.Null)
            {
                isError = true;
            }
            else if(_convertFrom.Type != _convertTo.Type)
            {
                return "Unit conversion:   Unit type mismatch.";
            }


            if(isError)
            {
                return "Unit conversion:   Cannot convert.";
            }
            else
            {
                decimal result = (_valueFrom * _convertFrom.Multiplier) / _convertTo.Multiplier;
                return string.Format("Unit conversion:   {0} {1} = {2} {3}", _valueFrom, _convertFrom.Name, result.ToString("0.#####"), _convertTo.Name);
            }
        }

        public bool Execute(string arguments)
        {
            // do nothing
            return false;
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

    /// <summary>
    /// Command for in-line equations
    /// </summary>
    internal class CalculatorCommand : RbCommand
    {
        private string _result;

        internal CalculatorCommand()
        {
            _result = string.Empty;
        }

        public string GetResponse(string arguments)
        {
            try
            {
                Expression expr = new Expression(arguments);
                _result = Convert.ToDecimal(expr.Evaluate()).ToString();
            }
            catch(Exception)
            {
                _result = "Error";
            }

            return string.Format("=   {0}", _result);
        }

        // copy the calculated value to the clipboard
        public bool Execute(string arguments)
        {
            System.Windows.Clipboard.SetText(_result);
            return false;
        }
    }
}
