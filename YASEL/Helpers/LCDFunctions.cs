using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        #region LCDFunctions
        //# Requires SetupStatics
        //# Requires BlockFunctions

        static int DebugLevel;
        static string LCDDebugScreenName;
        public static void dbug(string message, int debugLevel = 1, bool append = true, bool scroll = false)
        {
            if (DebugLevel == null || LCDDebugScreenName == null)
                return;
            if (debugLevel <= DebugLevel)
                if (scroll) DisplayOnScreen(LCDDebugScreenName, message, append, 16, true);
                else DisplayOnScreen(LCDDebugScreenName, message, append);
        }
        public static void DisplayOnScreen(List<IMyTerminalBlock> lcds, string message, bool append = true)
        {
            lcds.ForEach(lcd =>
            {
                DisplayOnScreen(lcd.CustomName, message, append);
            });
        }

        public static void DisplayOnScreen(string screenName, string message, bool append = true, int linesPerScreen = 16, bool truncate = false)
        {
            CheckStatics();
            List<IMyTerminalBlock> LCDs = new List<IMyTerminalBlock>();
            gts.SearchBlocksOfName(screenName, LCDs, BelongsToGrid);
            string curMessage = "";
            if (LCDs.Count == 0)
                return;

            if (append)
            {
                for (int i = 0; i <= LCDs.Count - 1; i++)
                {
                    var LCD = (IMyTextPanel)GetBlock(screenName + (LCDs.Count > 1 ? " " + (i + 1) : ""));
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
                IMyTextPanel LCD = (IMyTextPanel)GetBlock(screenName + (LCDs.Count > 1 ? " " + (curScreen + 1) : ""));
                bool lastLine = i == lines.Length - 1;

                if ((LCD is IMyTextPanel) && LCD.IsFunctional)
                { LCD.WritePublicText(lines[i] + (lastLine ? "" : "\n"), true); LCD.ShowPublicTextOnScreen(); }

            }

        }

        
        #endregion
    }
}
