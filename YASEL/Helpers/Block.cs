using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Block
{
    using Str;
    /// <summary>
    /// Static class for Block-level functions
    /// </summary>
    static class Block
    {
        /// <summary>
        /// Checks if a functional block is enabled and working (ie. Turned on and powered)
        /// Also checks if terminal block is a functional block, returning true if not
        /// </summary>
        /// <param name="b">The terminal block to check</param>
        /// <param name="checkIsWorking">Set to false if you don't want to check block is working - Default:True</param>
        /// <returns>
        /// true - Enabled (and working)
        /// <br />
        /// false - Is not enabled (or not working)
        /// </returns>
        public static bool IsEnabled(IMyTerminalBlock b, bool checkIsWorking = true)
        {
            return (checkIsWorking ? b.IsWorking : true) && ((b is IMyFunctionalBlock) ? ((IMyFunctionalBlock)b).Enabled : true);
        }

        /// <summary>
        /// Retrieves details from DetailedInfo of a block
        /// </summary>
        /// <param name="b">Block to get detail from</param>
        /// <param name="match">String to match</param>
        /// <returns>if there is a colon (':'), part of a line after colon. Otherwise returns whole line.</returns>
        public static string GetDetail(IMyTerminalBlock b, string match)
        {
            string requestedDetail = "";
            string[] lines = b.DetailedInfo.Split(new char[] { '\n' });
            Array.ForEach(lines, line =>
            {
                if (Str.Contains(line, match))
                {
                    string[] vals = line.Split(new char[] { ':' });
                    if (vals.Length > 1)
                        requestedDetail = vals[1].Trim();
                    else
                        requestedDetail = line;
                }
            });
            
            return requestedDetail;
        }
        /// <summary>
        /// Turns a list of blocks On or Off
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="on">Default True (Turns Blocks On). Set to false to turn blocks off.</param>
        public static void TurnOnOff(List<IMyTerminalBlock> blocks, bool on = true) { for (int i = 0; i <= blocks.Count - 1; i++) { TurnOnOff(blocks[i], on); } }
        /// <summary>
        /// Turns a block on or off
        /// </summary>
        /// <param name="b"></param>
        /// <param name="on">Default True (Turns Block On). Set to false to turn block off.</param>
        public static void TurnOnOff(IMyTerminalBlock b, bool on = true)
        { if (b.IsFunctional)b.GetActionWithName("OnOff_" + (on ? "On" : "Off")).Apply(b); }
    }
}