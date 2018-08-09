using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LiteDB;

namespace Rocketbox
{
    internal static class RbData
    {
        private static LiteDatabase _db;
        private static string _locale;

        // Regional search engine support
        private static string[] _allowedLocales = { "ca", "us", "uk" };

        // Master list of search engines
        internal static List<RbSearchEngine> SearchEngines;

        // Master list of converter units
        internal static List<RbConversionUnit> ConversionUnits;

        // Master list of Google Translate languages
        internal static List<RbTranslateLanguage> TranslateLanguages;

        // Master list of installed packages
        internal static List<string> Packages;

        // Universal time/date format
        private static string _dateFmt = "dddd, MMMM d, yyyy  ―  h:mm tt";
        internal static string DateFormat { get { return _dateFmt; } }

        private static bool isLoaded = false;

        /// <summary>
        /// Loads the Rocketbox database
        /// </summary>
        private static void LoadDatabase()
        {
            // TODO: errors etc
            _db = new LiteDatabase("Rocketbox.db");

            // Tries to read the locale from a local file
            // If first run/file is broken, assume Canada
            if(File.Exists("locale"))
            {
                string localeFileText = File.ReadAllText("locale").Trim().ToLower();
                if(_allowedLocales.Contains(localeFileText))
                {
                    _locale = localeFileText;
                }
                else
                {
                    _locale = "ca";
                }
            }
            else
            {
                File.WriteAllText("locale", "ca");
                _locale = "ca";
            }

            SearchEngines = _db.GetCollection<RbSearchEngine>("searchengines_" + _locale).FindAll().ToList<RbSearchEngine>();
            SearchEngines.AddRange(_db.GetCollection<RbSearchEngine>("searchengines").FindAll().ToList<RbSearchEngine>());

            ConversionUnits = _db.GetCollection<RbConversionUnit>("conversionunits").FindAll().ToList<RbConversionUnit>();
            TranslateLanguages = _db.GetCollection<RbTranslateLanguage>("languages").FindAll().ToList<RbTranslateLanguage>();

            Packages = new List<string>();

            foreach(RbSearchEngine engines in SearchEngines)
            {
                if(!Packages.Contains(engines.Collection.ToLower()))
                {
                    Packages.Add(engines.Collection.ToLower());
                }
            }
        }

        /// <summary>
        /// Must be called by the app before doing anything
        /// </summary>
        internal static void LoadData()
        {
            if(!isLoaded)
            {
                LoadDatabase();

                isLoaded = true;
            }
        }

        /// <summary>
        /// Searches the Rocketbox database for conversion units
        /// </summary>
        /// <param name="keyword">The keyword of the unit</param>
        /// <returns>The unit, or a null unit if invalid</returns>
        internal static RbConversionUnit GetConversionUnit(string keyword)
        {
            keyword = keyword.ToUpper();

            var results = from unit in ConversionUnits
                          where unit.Keywords.Contains(keyword)
                          select unit;

            if(results.Count() > 0)
            {
                return results.First();
            }
            else
            {
                return new RbConversionUnit { Name = "null", Multiplier = 0, Type = "null" };
            }
        }

        // ------------------------------------
        // Database modification
        // ------------------------------------

        /// <summary>
        /// Attempts to install a search engine pack
        /// </summary>
        /// <param name="packName">The name of the .rbx file in the Rocketbox directory</param>
        /// <returns></returns>
        internal static bool InstallSearchPack(string packName)
        {
            // sanity check
            if(!File.Exists(packName + ".rbx"))
            {
                return false;
            }

            string[] lines = File.ReadAllLines(packName + ".rbx");
            List<RbSearchEngine> newItems = new List<RbSearchEngine>();

            /*
             * .rbx line format:
             *      Name of Search Engine;;alias,alias,alias;;https://search.prefix/?q=
             */

            foreach(string line in lines)
            {
                if(!RbUtility.ValidPackLine(line))
                {
                    return false;
                }

                string[] fields = line.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                RbSearchEngine thisEngine = new RbSearchEngine();
                thisEngine.Name = fields[0];

                List<string> aliases = new List<string>();
                foreach(string alias in fields[1].Split(','))
                {
                    aliases.Add(alias.ToUpper());
                }

                thisEngine.Aliases = aliases.ToArray<string>();
                thisEngine.Url_Prefix = fields[2];
                thisEngine.Icon = string.Empty;

                thisEngine.Collection = packName;

                newItems.Add(thisEngine);
            }

            if(_db.GetCollection<RbSearchEngine>("searchengines").InsertBulk(newItems) != 0)
            {
                isLoaded = false;
                LoadData();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to remove a search engine pack
        /// </summary>
        /// <param name="packName">The name of the pack</param>
        /// <returns></returns>
        internal static bool UninstallSearchPack(string packName)
        {
            // sanity check
            if(!Packages.Contains(packName.ToLower()))
            {
                return false;
            }

            var dbCollection = _db.GetCollection<RbSearchEngine>("searchengines");
            foreach(RbSearchEngine engine in SearchEngines)
            {
                if(engine.Collection.ToLower() == packName.ToLower())
                {
                    dbCollection.Delete(engine.Id);
                }
            }

            isLoaded = false;
            LoadData();

            return true;
        }

        /// <summary>
        /// Writes the list of installed packs to RocketboxPackages.txt
        /// </summary>
        internal static void DumpInstalledPacks()
        {
            string dt = DateTime.Now.ToString("d MMM yyyy - HH:mm:ss");

            List<string> lines = new List<string>();
            lines.Add("Rocketbox - Installed Search Packages");
            lines.Add("(as of " + dt + ")");
            lines.Add("------------------------------------------");
            lines.AddRange(Packages);

            File.WriteAllLines("RocketboxPackages.txt", lines);
        }
    }

    /// <summary>
    /// Bundles information for a certain search engine
    /// </summary>
    internal class RbSearchEngine
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }
        public string Url_Prefix { get; set; }
        public string[] Aliases { get; set; }
        public string Icon { get; set; }
        public string Collection { get; set; }
    }

    /// <summary>
    /// Unit for the inline converter
    /// </summary>
    internal class RbConversionUnit
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Multiplier { get; set; }
        public string[] Keywords { get; set; }

        public RbUnitType GetUnitType()
        {
            switch (Type)
            {
                case "DIST":
                    return RbUnitType.Distance;
                case "VOL":
                    return RbUnitType.Volume;
                case "MASS":
                    return RbUnitType.Mass;
                case "DATA":
                    return RbUnitType.Data;
                default:
                    return RbUnitType.Null;
            }
        }
    }

    /// <summary>
    /// Google Translate language definition
    /// </summary>
    internal class RbTranslateLanguage
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string[] Keywords { get; set; }
    }
}
