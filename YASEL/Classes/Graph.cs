using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Graph
{

    /// <summary>
    /// Functions for making text graphs for TextPanels
    /// </summary>
    static class Graph
    { 
        public static string PrepareBarGraph(Dictionary<string, double> levels, Dictionary<string, double> curLevels, double widthMod = 0.54, string showValAs = "percent")
        {
            var curLevelsEnum = curLevels.GetEnumerator();
            string output = "";
            while (curLevelsEnum.MoveNext())
            {
                var k = curLevelsEnum.Current.Key;
                double curLvl = curLevelsEnum.Current.Value;
                
                if (levels.ContainsKey(k) && levels[k] != 0)
                {
                    int percentOfLevel = (int)(curLvl / levels[k] * 100);
                    int modLevel = (int)(Math.Min(100, percentOfLevel) / widthMod);
                    string strVal = "";
                    if (showValAs == "percent")
                        strVal = (int)percentOfLevel + "%";
                    else if (showValAs == "level")
                        strVal = Math.Round((levels[k] / 1000.0), 1) + "k";
                    else if (showValAs == "curLevel")
                        strVal = Math.Round((curLvl / 1000.0), 1) + "k";
                    else if (showValAs == "levelLessCurLevel")
                        strVal = Math.Round(((levels[k] - curLvl) / 1000.0), 1) + "k";

                    output += k + ": (" + strVal + ")\n";
                    for (int i = 1; i <= modLevel; i++)
                    {
                        output += "|";
                    }
                    output += "\n";
                }
            }
            return output;
        }
    }
    
}
