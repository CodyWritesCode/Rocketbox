using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Rocketbox
{
    internal static class RbData
    {
        // Master list of search engines
        internal static List<RbSearchEngine> SearchEngines;

        // Master list of converter units
        internal static List<RbConversionUnit> ConversionUnits;

        private static bool isLoaded = false;

        // Must be called by app before doing anything
        internal static void LoadData()
        {
            if(!isLoaded)
            {
                // load search engines from config
                StreamReader reader = new StreamReader("SearchEngines.cfg");
                string fileContents = reader.ReadToEnd();
                reader.Close();

                string[] segments = fileContents.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                SearchEngines = new List<RbSearchEngine>();

                /*  Config format:
                 *      Google                            Name of engine
                 *      https://www.google.ca/search?q=   URL prefix to append parameters to
                 *      GOOGLE                            Keyword
                 *      GOOG                              Keyword
                 *      GL                                Keyword
                 *      ;;                                Delimeter
                 * */

                foreach(string segment in segments)
                {
                    string[] lines = segment.Trim().Split('\n');
                    string[] keywords = lines.Skip(2).ToArray();
                    try
                    {
                        SearchEngines.Add(new RbSearchEngine(lines[0], lines[1], keywords));
                    }
                    catch(Exception e)
                    {
                        RbUtility.ThrowConfigError("Search engine configuration");
                    }
                }

                // load conversion units from config

                reader = new StreamReader("ConversionUnits.cfg");
                fileContents = reader.ReadToEnd();
                reader.Close();

                segments = fileContents.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries);

                ConversionUnits = new List<RbConversionUnit>();

                /*  Config format:
                 *      Centimeters                       Name of unit
                 *      DIST                              Type of unit (DIST = distance)
                 *      10                                Multiplier (number of base units comprising this unit)
                 *      CENTIMETER                        Keyword
                 *      CENTIMETRE                        Keyword
                 *      CENTIMETERS                       Keyword
                 *      CENTIMETRES                       Keyword
                 *      CM                                Keyword
                 *      ;;                                Delimeter
                 * */

                foreach(string segment in segments)
                {
                    string[] lines = segment.Trim().Split('\n');
                    string[] keywords = lines.Skip(3).ToArray();

                    RbUnitType type = RbUnitType.Null;

                    switch(lines[1])
                    {
                        case "DIST":
                            type = RbUnitType.Distance;
                            break;
                        case "VOL":
                            type = RbUnitType.Volume;
                            break;
                    }

                    try
                    {
                        ConversionUnits.Add(new RbConversionUnit(lines[0], type, Double.Parse(lines[2]), keywords));
                    }
                    catch(Exception e)
                    {
                        RbUtility.ThrowConfigError("Conversion unit config");
                    }

                    if (type == RbUnitType.Null)
                    {
                        RbUtility.ThrowConfigError("Conversion unit config - invalid type for " + lines[0]);
                    }
                }


                isLoaded = true;
            }
        }


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
                return new RbConversionUnit("null", RbUnitType.Null, 0, "null");
            }
        }
    }

    /// <summary>
    /// Bundles information for a certain search engine
    /// </summary>
    internal struct RbSearchEngine
    {
        internal string Name { get; private set; }
        internal string SearchUrl { get; private set; }
        internal string[] Keywords { get; private set; }
       
        internal RbSearchEngine(string name, string searchUrl, params string[] keywords)
        {
            Name = name;
            SearchUrl = searchUrl;

            Keywords = keywords;
        }
    }

    /// <summary>
    /// Unit for the inline converter
    /// </summary>
    internal struct RbConversionUnit
    {
        internal string Name { get; private set; }
        internal RbUnitType Type { get; private set; }
        internal double Multiplier { get; private set; }
        internal string[] Keywords { get; private set; }

        internal RbConversionUnit(string name, RbUnitType type, double multiplier, params string[] keywords)
        {
            Name = name;
            Type = type;
            Multiplier = multiplier;
            Keywords = keywords;
        }
    }
}
