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
        #region BlockFunctions
        //#Requires SetupStatics
        //#Requires GenericFunctions

        public static bool BelongsToGrid(IMyTerminalBlock b)
        {
            CheckStatics();
            return b.CubeGrid == sMe.CubeGrid;
        }
        public static IMyTerminalBlock GetBlock(string name, bool onGrid = true)
        {
            CheckStatics();
            var blocks = new List<IMyTerminalBlock>();

            if (onGrid)
                gts.SearchBlocksOfName(name, blocks, BelongsToGrid);
            else
                gts.SearchBlocksOfName(name, blocks);
            if (blocks.Count == 0)
                return null;
            return blocks[0];
        }
        public static List<IMyTerminalBlock> SearchBlocks(string name, bool onGrid = true)
        {
            CheckStatics();
            var lst = new List<IMyTerminalBlock>();
            if (onGrid)
                gts.SearchBlocksOfName(name, lst, BelongsToGrid);
            else
                gts.SearchBlocksOfName(name, lst);
            return lst;
        }
        public static bool IsEnabled(IMyTerminalBlock b)
        {
            return ((IMyFunctionalBlock)b).Enabled && BelongsToGrid(b);
        }
        public static string GetDetail(IMyTerminalBlock b, string match)
        {
            string requestedDetail = "";
            string[] lines = b.DetailedInfo.Split(new char[] { '\n' });
            Array.ForEach(lines, line =>
            {
                if (InStrI(line, match))
                {
                    string[] vals = line.Split(new char[] { ':' });
                    if (vals.Length > 1)
                        requestedDetail = vals[1].Trim();
                }
            });
            return requestedDetail;
        }

        public static List<IMyTerminalBlock> GetBlockGrp(string grpName)
        {
            CheckStatics();
            var grp = gts.BlockGroups.Find(x => InStrI(x.Name, grpName));
            if (grp == null)
                return new List<IMyTerminalBlock>();
            else
                return grp.Blocks;
        }

        public static void TurnOnOff(List<IMyTerminalBlock> blocks, bool on = true) { for (int i = 0; i <= blocks.Count - 1; i++) { TurnOnOff(blocks[i], on); } }
        public static void TurnOnOff(IMyTerminalBlock b, bool on = true)
        { if (b.IsFunctional)b.GetActionWithName("OnOff_" + (on ? "On" : "Off")).Apply(b); }
        public static bool IsOn(IMyTerminalBlock b) { return (b as IMyFunctionalBlock).Enabled; }

        #endregion
    }
}
