using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TextPanel
{
    using Block;
    using GridHelper;
    /// <summary>
    /// Static class for common TextPanel functions
    /// </summary>
    static class TextPanel
    {
        /// <summary>
        /// Writes to an TextPanel
        /// </summary>
        /// <param name="textPanel"></param>
        /// <param name="text"></param>
        /// <param name="append"></param>
        /// <param name="publicText"></param>
        /// <example>
        /// TextPanel.Write(myTextPanel, "Write this text");
        /// </example>
        public static void Write(IMyTextPanel textPanel, string text, bool append = true, bool publicText = true)
        {
            if (textPanel == null)
                throw new Exception("TextPanel.Write: textPanel is null");
            if (publicText)
                textPanel.WritePublicText(text, append);
            else
                textPanel.WritePrivateText(text, append);
        }

        /// <summary>
        /// Writes to an TextPanel
        /// </summary>
        /// <param name="textPanelName"></param>
        /// <param name="text"></param>
        /// <param name="append"></param>
        /// <param name="publicText"></param>
        /// <example>
        /// TextPanel.Write("MyLCD", "Write this text");
        /// </example>
        public static void Write(GridHelper gh, string textPanelName, string text, bool append = true, bool publicText = true)
        {
            var textPanel = gh.GetBlock(textPanelName) as IMyTextPanel;
            if (textPanel == null)
                throw new Exception("TextPanel.Write: textPanel (" + textPanelName + "is not accessible");
            Write(textPanel, text, append, publicText);
        }

        /// <summary>
        /// Retrieves a list of line separted key/value pairs from an LCD. key/value separated by colon
        /// </summary>
        /// <param name="LCDName"></param>
        /// <returns></returns>
        public static Dictionary<String, double> GetValueListFromLCD(GridHelper gh, string LCDName)
        {
            Dictionary<String, double> valueList = new Dictionary<String, double>();

            IMyTextPanel lcd = (IMyTextPanel)gh.Gts.GetBlockWithName(LCDName);
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
