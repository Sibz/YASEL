using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace GridHelper
{
    /// <summary>
    /// Grid functions<br />
    /// 
    /// </summary>
    public class GridHelper
    {

        // Assemblable items
        public const string Components = "BulletproofGlass,Computer,Construction,Detector,Display,Girder,GravityGenerator,InteriorPlate,LargeTube,Medical,MetalGrid,Motor,PowerCell,RadioCommunication,Reactor,SmallTube,SolarCell,SteelPlate,Thrust,Explosives";
        public const string Ammos = "Missile200mm,NATO_25x184mm,NATO_5p56x45mm";
        public const string Tools = "WelderItem,HandDrillItem,AngleGrinderItem,AutomaticRifleItem";
        public const string OxygenContainers = "OxygenBottle";

        public GridHelper(MyGridProgram gp)
        {
            Gts = gp.GridTerminalSystem;
            Pb = gp.Me;
            Echo = gp.Echo;
            Program = gp;
        }

        public MyGridProgram Program;
        public IMyGridTerminalSystem Gts;

        public IMyProgrammableBlock Pb;

        public Action<string> Echo;

        /// <summary>
        /// Checks if block is on same grid as currently running PB
        /// </summary>
        /// <param name="b">Block</param>
        /// <returns>True if block on this grid</returns>
        public bool BelongsToGrid(IMyTerminalBlock b)
        {
            return b.CubeGrid == Pb.CubeGrid;
        }

        public IMyTerminalBlock GetBlock(string name, bool onGrid = true)
        {
            var blocks = new List<IMyTerminalBlock>();

            if (onGrid)
                Gts.SearchBlocksOfName(name, blocks, BelongsToGrid);
            else
                Gts.SearchBlocksOfName(name, blocks);
            if (blocks.Count == 0)
                return null;
            return blocks[0];
        }
        public List<IMyTerminalBlock> SearchBlocks(string name, bool onGrid = true)
        {
            var lst = new List<IMyTerminalBlock>();
            if (onGrid)
                Gts.SearchBlocksOfName(name, lst, BelongsToGrid);
            else
                Gts.SearchBlocksOfName(name, lst);
            return lst;
        }
        public List<IMyTerminalBlock> GetBlockGroup(string groupName)
        {
            List<IMyBlockGroup> grps = new List<IMyBlockGroup>();
            Gts.GetBlockGroups(grps);
            var grp = grps.Find(x => x.Name.Contains(groupName));
            if (grp == null)
                return new List<IMyTerminalBlock>();
            else
                return grp.Blocks;
        }
        public List<IMyTerminalBlock> GetBlockGrp(string grpName)
        {
            return GetBlockGroup(grpName);
        }

    }
}
