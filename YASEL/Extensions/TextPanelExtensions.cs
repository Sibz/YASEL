using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TextPanelExtensions
{
    /// <summary>
    /// Static class for common TextPanel functions
    /// </summary>
    static class TextPanelExtensions
    {
 
        /// <summary>
        /// Retrieves a list of line separted key/value pairs from an LCD. key/value separated by colon
        /// </summary>
        /// <param name="LCDName"></param>
        /// <returns></returns>
        public static Dictionary<String, double> GetValueList(this IMyTextPanel lcd)
        {
            Dictionary<String, double> valueList = new Dictionary<String, double>();

            if ((lcd is IMyTextPanel) && lcd.IsFunctional)
            {
                string strText = lcd.GetPublicText();
                string[] lines = strText.Split(new char[] { '\n' });
                Array.ForEach(lines, line =>
                {
                    if (line.Contains(":"))
                    {
                        string[] vals = line.Split(new char[] { ':' });
                        valueList.Add(vals[0], Convert.ToDouble(vals[1].Trim()));
                    }
                });
            }
            return valueList;
        }
    }
}
