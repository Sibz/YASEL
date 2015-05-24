using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;

namespace YASEL
{
    partial class Program
    {

        #region GenericFunctions
        //# Requires SetupStatics

        public static bool InStrI(string haystack, string needle) { return (haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0); }

        public static bool InStrsI(string haystack, string needle)
        {
            string[] needles = needle.Split(new char[] { ';' });
            return InStrsI(haystack, needles);
        }

        public static bool InStrsI(string haystack, string[] needles)
        {
            for (int nd = 0; nd <= needles.Length - 1; nd++)
            {
                if (haystack.IndexOf(needles[nd], StringComparison.CurrentCultureIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        public static Dictionary<String, double> GetValueListFromLCD(string LCDName)
        {
            Dictionary<String, double> valueList = new Dictionary<String, double>();

            IMyTextPanel lcd = (IMyTextPanel)gts.GetBlockWithName(LCDName);
            if ((lcd is IMyTextPanel) && lcd.IsFunctional)
            {
                string strText = lcd.GetPublicText();
                string[] lines = strText.Split(new char[] { '\n' });
                Array.ForEach(lines, line =>
                {
                    if (InStrI(line, ":"))
                    {
                        string[] vals = line.Split(new char[] { ':' });
                        valueList.Add(vals[0], Convert.ToDouble(vals[1].Trim()));
                    }
                });
            }
            return valueList;

        }

        #endregion
    }
}
