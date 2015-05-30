using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TextPanel
{
    using Str;
    using Block;
    using Grid;
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
        public static void Write(string textPanelName, string text, bool append = true, bool publicText = true)
        {
            var textPanel = Grid.GetBlock(textPanelName) as IMyTextPanel;
            if (textPanel == null)
                throw new Exception("TextPanel.Write: textPanel (" + textPanelName + "is not accessible");
            Write(textPanel, text, append, publicText);
        }

        /// <summary>
        /// Writes message to multiple screens
        /// </summary>
        /// <param name="lcds"></param>
        /// <param name="message"></param>
        /// <param name="append"></param>
        public static void WriteMulti(List<IMyTerminalBlock> lcds, string message, bool append = true)
        {
            lcds.ForEach(lcd =>
            {
                WriteMulti(lcd.CustomName, message, append);
            });
        }
        /// <summary>
        /// Write message to screen<br />
        /// If more than one screen matching name with number appended, i.e. 'LCD 1', 'LCD 2'; will write across those screens.
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="message"></param>
        /// <param name="append">Append to text already on screen. Default:true</param>
        /// <param name="linesPerScreen">How many lines to write on a screen before writing to the next one. Default:16</param>
        /// <param name="truncate">If number of lines exceeds the display capticity, trucate lines off the top. Default:false</param>
        public static void WriteMulti(string screenName, string message, bool append = true, int linesPerScreen = 16, bool truncate = false)
        {
            List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
            Grid.ts.SearchBlocksOfName(screenName, LCDs, Grid.BelongsToGrid);
            string curMessage = "";
            if (LCDs.Count == 0)
                throw new Exception("Unable to write to LCD, LCD '" + screenName + "' Not Found.");

            if (append)
            {
                for (int i = 0; i <= LCDs.Count - 1; i++)
                {
                    var LCD = (IMyTextPanel)Grid.GetBlock(screenName + (LCDs.Count > 1 ? " " + (i + 1) : ""));
                    if ((LCD is IMyTextPanel) && LCD.IsFunctional)
                        curMessage += ((IMyTextPanel)LCD).GetPublicText();
                }
            }
            LCDs.ForEach(LCD =>
            {
                ((IMyTextPanel)LCD).WritePublicText("", false);
            });


            string newMessage = curMessage + (curMessage == "" ? "" : "\n") + message;
            string[] lines = newMessage.Split(new char[] { '\n' });
            int numLines = lines.Length;
            int maxLines = truncate ? LCDs.Count * linesPerScreen : 999;
            int truncateLines = numLines > maxLines ? numLines - maxLines : 0;
            int curLineOnScreen = 0;
            int curScreen = 0;
            for (int i = truncateLines; i <= lines.Length - 1; i++)
            {
                if (curLineOnScreen >= linesPerScreen)
                { curLineOnScreen = 1; if (curScreen < LCDs.Count - 1) curScreen++; }
                else curLineOnScreen++;
                IMyTextPanel LCD = (IMyTextPanel)Grid.GetBlock(screenName + (LCDs.Count > 1 ? " " + (curScreen + 1) : ""));
                bool lastLine = i == lines.Length - 1;

                if ((LCD is IMyTextPanel) && LCD.IsFunctional)
                { LCD.WritePublicText(lines[i] + (lastLine ? "" : "\n"), true); LCD.ShowPublicTextOnScreen(); }

            }

        }
        /// <summary>
        /// Retrieves a list of line separted key/value pairs from an LCD. key/value separated by colon
        /// </summary>
        /// <param name="LCDName"></param>
        /// <returns></returns>
        public static Dictionary<String, double> GetValueListFromLCD(string LCDName)
        {
            Dictionary<String, double> valueList = new Dictionary<String, double>();

            IMyTextPanel lcd = (IMyTextPanel)Grid.ts.GetBlockWithName(LCDName);
            if ((lcd is IMyTextPanel) && lcd.IsFunctional)
            {
                string strText = lcd.GetPublicText();
                string[] lines = strText.Split(new char[] { '\n' });
                Array.ForEach(lines, line =>
                {
                    if (Str.Contains(line, ":"))
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
