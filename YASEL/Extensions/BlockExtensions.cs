using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace BlockExtensions
{
    /// <summary>
    /// Static class for Block-level functions
    /// </summary>
    static class BlockExtensions
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
        public static bool IsEnabled(this IMyTerminalBlock b, bool checkIsWorking = true)
        {
            return (checkIsWorking ? b.IsWorking : true) && ((b is IMyFunctionalBlock) ? ((IMyFunctionalBlock)b).Enabled : true);
        }
        public static bool AreEnabled(this List<IMyTerminalBlock> blocks, bool checkIsWorking = true)
        {
            bool rval = true;
            blocks.ForEach(b => { rval &= b.IsEnabled(); });
            return rval;
        }

        /// <summary>
        /// Retrieves details from DetailedInfo of a block
        /// </summary>
        /// <param name="b">Block to get detail from</param>
        /// <param name="match">String to match</param>
        /// <returns>if there is a colon (':'), part of a line after colon. Otherwise returns whole line.</returns>
        public static string GetDetail(this IMyTerminalBlock b, string match)
        {
            string requestedDetail = "";
            string[] lines = b.DetailedInfo.Split(new char[] { '\n' });
            Array.ForEach(lines, line =>
            {
                if (line.Contains(match))
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
        /// Turns a list of blocks On
        /// </summary>
        /// <param name="blocks"></param>
        public static void TurnOn(this List<IMyTerminalBlock> blocks) 
        {
            blocks.ForEach(b => { b.TurnOn(); });
        }
        /// <summary>
        /// Turns a block on
        /// </summary>
        /// <param name="b"></param>
        public static void TurnOn(this IMyTerminalBlock b)
        { 
            if (b.IsFunctional)b.GetActionWithName("OnOff_On").Apply(b);
        }
        /// <summary>
        /// Turns a list of blocks Off
        /// </summary>
        /// <param name="blocks"></param>
        public static void TurnOff(this List<IMyTerminalBlock> blocks)
        {
            blocks.ForEach(b => { b.TurnOff(); });
        }
        /// <summary>
        /// Turns a block off
        /// </summary>
        /// <param name="b"></param>
        public static void TurnOff(this IMyTerminalBlock b)
        {
            if (b.IsFunctional) b.GetActionWithName("OnOff_Off").Apply(b);
        }
    }

}