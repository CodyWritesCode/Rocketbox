using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LiteDB;

namespace Rocketbox
{
    internal static class RbData
    {
        private static LiteDatabase _db;

        // Master list of search engines
        internal static List<RbSearchEngine> SearchEngines;

        // Master list of converter units
        internal static List<RbConversionUnit> ConversionUnits;

        // Master list of Google Translate languages
        internal static List<RbTranslateLanguage> TranslateLanguages;

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

            SearchEngines = _db.GetCollection<RbSearchEngine>("searchengines").FindAll().ToList<RbSearchEngine>();
            ConversionUnits = _db.GetCollection<RbConversionUnit>("conversionunits").FindAll().ToList<RbConversionUnit>();
            TranslateLanguages = _db.GetCollection<RbTranslateLanguage>("languages").FindAll().ToList<RbTranslateLanguage>();
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
    }

    /// <summary>
    /// Not used yet
    /// </summary>
    internal class RbSetting
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// Bundles information for a certain search engine
    /// </summary>
    internal class RbSearchEngine
    {
        public string Name { get; set; }
        public string Url_Prefix { get; set; }
        public string[] Aliases { get; set; }
        public string Icon { get; set; }
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
