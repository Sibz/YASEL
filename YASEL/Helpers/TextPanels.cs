using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace TextPanels
{
    using Block;
    using GridHelper;
    /// <summary>
    /// Static class for writing to multiple TextPanels
    /// </summary>
    static class TextPanels
    {

        /// <summary>
        /// Write message to screen<br />
        /// If more than one screen matching name with number appended, i.e. 'LCD 1', 'LCD 2'; will write across those screens.
        /// </summary>
        /// <param name="screenName"></param>
        /// <param name="message"></param>
        /// <param name="append">Append to text already on screen. Default:true</param>
        /// <param name="linesPerScreen">How many lines to write on a screen before writing to the next one. Default:16</param>
        /// <param name="truncate">If number of lines exceeds the display capticity, trucate lines off the top. Default:false</param>
        public static void Write(GridHelper gh, string screenName, string message, bool append = true, int linesPerScreen = 16, bool truncate = false)
        {
            List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
            gh.Gts.SearchBlocksOfName(screenName, LCDs, gh.BelongsToGrid);
            string curMessage = "";
            if (LCDs.Count == 0)
                throw new Exception("Unable to write to LCD, LCD '" + screenName + "' Not Found.");

            if (append)
            {
                for (int i = 0; i <= LCDs.Count - 1; i++)
                {
                    var LCD = (IMyTextPanel)gh.GetBlock(screenName + (LCDs.Count > 1 ? " " + (i + 1) : ""));
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
                IMyTextPanel LCD = (IMyTextPanel)gh.GetBlock(screenName + (LCDs.Count > 1 ? " " + (curScreen + 1) : ""));
                bool lastLine = i == lines.Length - 1;

                if ((LCD is IMyTextPanel) && LCD.IsFunctional)
                { LCD.WritePublicText(lines[i] + (lastLine ? "" : "\n"), true); LCD.ShowPublicTextOnScreen(); }

            }

        }
    }
}
