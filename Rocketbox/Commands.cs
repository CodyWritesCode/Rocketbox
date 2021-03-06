﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
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

        // gets the icon in the /icons directory, if any
        string GetIcon();
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

        public string GetIcon()
        {
            return string.Empty;
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

        public string GetIcon()
        {
            return _engine.Icon;
        }
    }

    /// <summary>
    /// Command for converting between units of measurement.
    /// </summary>
    internal class UnitConversionCommand : RbCommand
    {
        private RbConversionUnit _convertFrom;
        private RbConversionUnit _convertTo;

        private decimal _valueFrom;

        private decimal _result;

        internal UnitConversionCommand() { }

        public string GetResponse(string arguments)
        {
            bool isError = false;

            string[] parts = arguments.Split(' ');

            if (parts.Length < 3)
            {
                return "Unit conversion:   Not enough parameters.";
            }

            // first and second part = number/unit
            if (!decimal.TryParse(parts[0], out _valueFrom))
            {
                isError = true;
            }

            _convertFrom = RbData.GetConversionUnit(parts[1]);

            // last part = output unit
            // (can use "to" or other combos in the middle)
            _convertTo = RbData.GetConversionUnit(parts.Last());


            if (_convertFrom.GetUnitType() == RbUnitType.Null || _convertTo.GetUnitType() == RbUnitType.Null)
            {
                isError = true;
            }
            else if (_convertFrom.Type != _convertTo.Type)
            {
                return "Unit conversion:   Unit type mismatch.";
            }


            if (isError)
            {
                return "Unit conversion:   Cannot convert.";
            }
            else
            {
                _result = (_valueFrom * _convertFrom.Multiplier) / _convertTo.Multiplier;
                return string.Format("Unit conversion:   {0} {1} = {2} {3}", _valueFrom, _convertFrom.Name, _result.ToString("0.#####"), _convertTo.Name);
            }
        }

        public bool Execute(string arguments)
        {
            System.Windows.Clipboard.SetText(_result.ToString("0.#####"));
            return false;
        }

        public string GetIcon()
        {
            return string.Empty;
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
            catch (Exception)
            {
                // if launch fails, just do nothing
                return false;
            }

            return true;
        }

        public string GetIcon()
        {
            return string.Empty;
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
                // Math is processed by ncalc
                Expression expr = new Expression(arguments);
                _result = Convert.ToDecimal(expr.Evaluate()).ToString();
            }
            catch (Exception)
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

        public string GetIcon()
        {
            return "calculator.png";
        }
    }

    /// <summary>
    /// Command for launching Google Translate
    /// </summary>
    internal class TranslateCommand : RbCommand
    {
        private RbTranslateLanguage _fromLang;
        private RbTranslateLanguage _toLang;
        private string _textToTranslate;

        internal TranslateCommand()
        {
            _fromLang = null;
            _toLang = null;
            _textToTranslate = "";
        }

        /// <summary>
        /// Determines the languages and then puts them into the command
        /// </summary>
        /// <param name="arguments">The language string</param>
        private void Decode(string arguments)
        {
            List<string> parts = arguments.Split(' ').ToList();

            if (parts.Count > 0)
            {
                var matchingLangs = from lang in RbData.TranslateLanguages
                                    where lang.Keywords.Contains(parts[0].ToUpper())
                                    select lang;
                if (matchingLangs.Count() > 0) { _fromLang = matchingLangs.First(); }
            }

            if (parts.Count > 1)
            {
                var matchingLangs = from lang in RbData.TranslateLanguages
                                    where lang.Keywords.Contains(parts[1].ToUpper())
                                    select lang;
                if (matchingLangs.Count() > 0) { _toLang = matchingLangs.First(); }
            }

            // finds the string to translate (starts at index 2 of the array)
            if (parts.Count > 2)
            {
                parts.RemoveRange(0, 2);
                _textToTranslate = string.Join(" ", parts);
            }

        }

        public string GetResponse(string arguments)
        {
            Decode(arguments);

            string nameA = "Unknown", nameB = "Unknown";

            if (_fromLang != null) { nameA = _fromLang.Name; }
            if (_toLang != null) { nameB = _toLang.Name; }

            return String.Format("Translate {0} to {1}: \"{2}\"", nameA, nameB, _textToTranslate);
        }

        public bool Execute(string arguments)
        {
            Decode(arguments);

            if (_fromLang == null || _toLang == null)
            {
                return false;
            }
            else
            {
                string translateUrl = string.Format("https://translate.google.com/#{0}/{1}/{2}", _fromLang.Code, _toLang.Code, _textToTranslate);
                Process.Start(translateUrl);
                return true;
            }
        }

        public string GetIcon()
        {
            return "translate.png";
        }
    }

    /// <summary>
    /// Command for simply displaying a date/time
    /// </summary>
    internal class TimeCommand : RbCommand
    {

        internal TimeCommand() { }

        public string GetResponse(string arguments)
        {
            string timeStr = string.Format("Current date/time:   {0}", DateTime.Now.ToString(RbData.DateFormat));

            return timeStr;
        }

        public bool Execute(string arguments)
        {
            return false;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command for finding differences from the current time/date
    /// </summary>
    internal class TimeDiffCommand : RbCommand
    {
        private RbTimeDiffMode _mode;

        internal TimeDiffCommand(RbTimeDiffMode mode)
        {
            _mode = mode;
        }

        public string GetResponse(string arguments)
        {
            bool error = false;
            bool beforeCE = false;
            int beforeCEYear = 0;
            string errorString = "Cannot compute date/time.";

            if (arguments.Trim() == string.Empty) { errorString = "Add/subtract date/time..."; }

            string[] parts = arguments.ToUpper().Split(' ');

            DateTime calcDate = DateTime.Now;

            // goes through each part of the arguments to decipher the units/amount of time specified
            foreach (string s in parts)
            {
                int diff = 0;
                try
                {
                    diff = int.Parse(Regex.Match(s, @"\d+").Value);
                }
                catch { error = true; }

                // whether we're adding to or subtracting from the current date/time
                if (_mode == RbTimeDiffMode.Subtract)
                {
                    diff = -diff;
                }

                if (s.EndsWith("MI") || s.EndsWith("MIN") || s.EndsWith("MINS") || s.EndsWith("MINUTE") || s.EndsWith("MINUTES"))
                {
                    calcDate = calcDate.AddMinutes(diff);
                }
                else if (s.EndsWith("H") || s.EndsWith("HR") || s.EndsWith("HRS") || s.EndsWith("HOUR") || s.EndsWith("HOURS"))
                {
                    calcDate = calcDate.AddHours(diff);
                }
                else if (s.EndsWith("D") || s.EndsWith("DAY") || s.EndsWith("DAYS"))
                {
                    calcDate = calcDate.AddDays(diff);
                }
                else if (s.EndsWith("MO") || s.EndsWith("MONTH") || s.EndsWith("MONTHS"))
                {
                    calcDate = calcDate.AddMonths(diff);
                }
                else if (s.EndsWith("Y") || s.EndsWith("YR") || s.EndsWith("YRS") || s.EndsWith("YEAR") || s.EndsWith("YEARS"))
                {
                    try
                    {
                        calcDate = calcDate.AddYears(diff);
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        if(_mode == RbTimeDiffMode.Subtract)
                        {
                            beforeCE = true;
                            beforeCEYear = -(DateTime.Now.Year + diff);
                        }
                    }
                }
                else
                {
                    error = true;
                }
            }

            if (error)
            {
                return errorString;
            }
            else
            {
                if(!beforeCE)
                {
                    return string.Format("Calculated date/time:   {0}", calcDate.ToString(RbData.DateFormat));
                }
                else
                {
                    return string.Format("Calculated date/time:   {0}, {1} BCE  ―  {2}", calcDate.ToString("MMMM d"), beforeCEYear, calcDate.ToString("h:mm tt"));
                }
            }
        }

        public bool Execute(string arguments)
        {
            return false;
        }

        public string GetIcon()
        {
            return string.Empty;
        }

    }

    /// <summary>
    /// Command for comparing the difference between now and another date
    /// </summary>
    internal class TimeCompareCommand : RbCommand
    {
        public TimeCompareCommand() { }

        public string GetResponse(string arguments)
        {
            string output = "Time since/until...";
            bool error = false;

            DateTime dtToCompare = DateTime.Now;

            try
            {
                dtToCompare = DateTime.Parse(arguments);
            }
            catch { error = true; }

            TimeSpan diff;

            if (dtToCompare > DateTime.Now)
            {
                output = string.Format("Time until {0}:  ", dtToCompare.ToString(RbData.DateFormat));
                diff = dtToCompare - DateTime.Now;
            }
            else
            {
                output = string.Format("Time since {0}:  ", dtToCompare.ToString(RbData.DateFormat));
                diff = DateTime.Now - dtToCompare;
            }

            // to save space, only displaying units that have a non-zero value
            int years = 0;
            while (diff.Days >= 365)
            {
                diff = diff.Subtract(new TimeSpan(365, 0, 0, 0));
                years++;
            }
            if (years > 0)
            {
                output += string.Format(" {0} years", years);
            }

            if (diff.Days > 0)
            {
                output += string.Format(" {0} days", diff.Days);
            }

            if (diff.Hours > 0)
            {
                output += string.Format(" {0} hours", diff.Hours);
            }

            if (diff.Minutes > 0)
            {
                output += string.Format(" {0} minutes", diff.Minutes);
            }

            if (error) { output = "Time since/until:   Unable to parse date."; }

            return output;
        }

        public bool Execute(string arguments)
        {
            return false;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command for getting the current Unix timestamp
    /// </summary>
    internal class UnixTimeCommand : RbCommand
    {
        private string _unixTimeString;

        public UnixTimeCommand() { }

        public bool Execute(string arguments)
        {
            System.Windows.Clipboard.SetText(_unixTimeString);
            return false;
        }

        public string GetResponse(string arguments)
        {
            _unixTimeString = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            return "Current Unix timestamp:  " + _unixTimeString;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command for converting Unix time into the local date/time
    /// </summary>
    internal class FromUnixTimeCommand : RbCommand
    {
        private long _epochValue;
        private string _dateString;

        public FromUnixTimeCommand() { }

        public bool Execute(string arguments)
        {
            if(_dateString != string.Empty)
            {
                System.Windows.Clipboard.SetText(_dateString);
            }

            return false;
        }

        public string GetResponse(string arguments)
        {
            _dateString = string.Empty;

            if(arguments.Trim() == string.Empty)
            {
                return "Convert from Unix time...";
            }

            if(!long.TryParse(arguments, out _epochValue))
            {
                return "Unable to parse Unix time.";
            }

            try
            {
                DateTimeOffset dt = DateTimeOffset.FromUnixTimeSeconds(_epochValue);
                _dateString = "Local time:   " + dt.ToLocalTime().ToString(RbData.DateFormat);
            }
            catch(ArgumentOutOfRangeException)
            {
                _dateString = "Unable to parse Unix time.";
            }

            return _dateString;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command to convert a date/time into Unix time
    /// </summary>
    internal class ToUnixTimeCommand : RbCommand
    {
        private DateTime _dt;
        private long _epochValue;
        private bool _goodConversion;

        public bool Execute(string arguments)
        {
            if(_goodConversion)
            {
                System.Windows.Clipboard.SetText(_epochValue.ToString());
            }
            return false;
        }

        public string GetResponse(string arguments)
        {
            _goodConversion = false;

            if(arguments.Trim() == string.Empty)
            {
                _goodConversion = false;
                return "Convert to Unix time...";
            }

            try
            {
                _dt = DateTime.Parse(arguments);
            }
            catch { return "Unable to parse date/time."; }

            _epochValue = ((DateTimeOffset)_dt).ToUnixTimeSeconds();

            _goodConversion = true;
            return "Unix time:  " + _epochValue;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    // -------------------------------------------------------
    // System commands
    // -------------------------------------------------------
    internal class ExitCommand : RbCommand
    {
        public ExitCommand() { }

        public bool Execute(string arguments)
        {
            Invoker.ShutdownNow = true;
            return true;
        }

        public string GetResponse(string arguments)
        {
            return "Shut down Rocketbox...";
        }

        public string GetIcon()
        {
            return "rocket.ico";
        }
    }

    // -------------------------------------------------------
    // Database modification
    // -------------------------------------------------------

    /// <summary>
    /// Command to dump list of installed search engine packs
    /// </summary>
    internal class ListPackCommand : RbCommand
    {
        public ListPackCommand() { }
        
        public bool Execute(string arguments)
        {
            RbData.DumpInstalledPacks();
            new ShellCommand("notepad.exe RocketboxPackages.txt").Execute("");
            return true;
        }

        public string GetResponse(string arguments)
        {
            return "List installed search engine packs...";
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command to install a search engine pack
    /// </summary>
    internal class InstallPackCommand : RbCommand
    {
        private bool _goodFile = false;

        private bool _installAttempted = false;
        private bool _installSuccessful = false;

        public InstallPackCommand() { }

        public bool Execute(string arguments)
        {
            if(_goodFile)
            {
                _installAttempted = true;

                if(RbData.InstallSearchPack(arguments))
                {
                    _installSuccessful = true;
                }
                else
                {
                    _installSuccessful = false;
                }
            }

            return false;
        }

        public string GetResponse(string arguments)
        {
            _goodFile = false;

            string output;

            if(_installAttempted)
            {
                if(_installSuccessful)
                {
                    output = "Successfully installed \"" + arguments + "\".";
                }
                else
                {
                    output = "Was not able to install \"" + arguments + "\". Please verify that the format is correct.";
                }

                _installAttempted = false;
            }
            else if(RbData.Packages.Contains(arguments.ToLower()))
            {
                output = "Pack \"" + arguments + "\" is already installed.";
            }
            else if(arguments.Trim() == string.Empty || arguments.Contains(" "))
            {
                output = "Install search engine pack...";
            }
            else if(arguments.ToLower().Contains("default"))
            {
                output = "Cannot add default packs.";
            }
            else if(System.IO.File.Exists(arguments + ".rbx"))
            {
                output = "Install search engine pack:  " + arguments;
                _goodFile = true;
            }
            else
            {
                output = "Search engine pack not found.";
                _goodFile = false;
            }

            return output;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Command to remove a search engine pack
    /// </summary>
    internal class UninstallPackCommand : RbCommand
    {
        private bool _packExists = false;
        private bool _uninstallSuccessful = false;

        public UninstallPackCommand() { }

        public bool Execute(string arguments)
        {
            if(_packExists)
            {
                RbData.UninstallSearchPack(arguments.Trim().ToLower());
                _uninstallSuccessful = true;
            }

            return false;
        }

        public string GetResponse(string arguments)
        {
            _packExists = false;
            _uninstallSuccessful = false;

            string output;

            if(_uninstallSuccessful)
            {
                output = "Successfully uninstalled \"" + arguments + "\".";
            }
            else if(arguments.Trim() == string.Empty || arguments.Contains(" "))
            {
                output = "Uninstall a search engine pack...";
            }
            else if(arguments.ToLower().Contains("default"))
            {
                output = "Cannot remove a default pack.";
            }
            else if(!RbData.Packages.Contains(arguments.Trim().ToLower()))
            {
                output = "Pack \"" + arguments + "\" is not installed.";
            }
            else
            {
                output = "Uninstall search engine pack:  " + arguments;
                _packExists = true;
            }

            return output;
        }

        public string GetIcon()
        {
            return string.Empty;
        }
    }
}
