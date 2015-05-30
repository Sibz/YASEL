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
using VRage;
using VRageMath;


namespace Grid
{
    using Str;
    /// <summary>
    /// Grid functions
    /// </summary>
    static class Grid
    {
        static public void Set(IMyGridTerminalSystem l_ts, IMyProgrammableBlock l_pb, Action<string> l_echo)
        {
            ts = l_ts;
            pb = l_pb;
            echo = l_echo;
        }
        static public IMyGridTerminalSystem ts { 
            get
            {
                if (ts == null)
                    throw new Exception("Static Grid Terminal System Not Set, set with 'Grid.ts = GridTerminalSystem' in Main");
                return ts;
            }
            set { ts = value ;}
        }

        static public IMyProgrammableBlock pb 
        { 
            get
            {
                if (pb == null)
                    throw new Exception("Static Programmable Block not set, set with 'Grid.pb = Me' in Main");
                return pb;
            }        
            set { pb = value;} 
        }

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
        public static List<IMyTerminalBlock> GetBlocks<T>(bool onGrid = true)
        {
            var lst = new List<IMyTerminalBlock>();
            if (onGrid)
                ts.GetBlocksOfType<T>(lst,BelongsToGrid);
            else
                ts.GetBlocksOfType<T>(lst);
            return lst;
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
