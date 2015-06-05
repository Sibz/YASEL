using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Grid
{
    using Str;
    /// <summary>
    /// Grid functions<br />
    /// To use you must call Grid.Set(GridTerminalSystem, Me, Echo); from your Main function.
    /// </summary>
    public static class Grid
    {
        static public void Set(MyGridProgram gp)
        {
            ts = gp.GridTerminalSystem;
            pb = gp.Me;
            echo = gp.Echo;
        }
        static public IMyGridTerminalSystem ts;

        static public IMyProgrammableBlock pb;

        static public Action<string> echo {get;set;}
        static public void Echo(string s)
        {
            if (echo == null)
                throw new Exception("Static Echo function not set, set with 'Grid.echo = Echo' in Main");
            echo(s);
        }

        /// <summary>
        /// Checks if block is on same grid as currently running PB
        /// </summary>
        /// <param name="b">Block</param>
        /// <returns>True if block on this grid</returns>
        public static bool BelongsToGrid(IMyTerminalBlock b)
        {
            return b.CubeGrid == pb.CubeGrid;
        }
        public static IMyTerminalBlock GetBlock(string name, bool onGrid = true)
        {
            var blocks = new List<IMyTerminalBlock>();

            if (onGrid)
                ts.SearchBlocksOfName(name, blocks, BelongsToGrid);
            else
                ts.SearchBlocksOfName(name, blocks);
            if (blocks.Count == 0)
                return null;
            return blocks[0];
        }
        public static List<IMyTerminalBlock> SearchBlocks(string name, bool onGrid = true)
        {
            var lst = new List<IMyTerminalBlock>();
            if (onGrid)
                ts.SearchBlocksOfName(name, lst, BelongsToGrid);
            else
                ts.SearchBlocksOfName(name, lst);
            return lst;
        }
        public static List<IMyTerminalBlock> GetBlockGrp(string grpName)
        {
            var grp = ts.BlockGroups.Find(x => Str.Contains(x.Name, grpName));
            if (grp == null)
                return new List<IMyTerminalBlock>();
            else
                return grp.Blocks;
        }

    }
}
