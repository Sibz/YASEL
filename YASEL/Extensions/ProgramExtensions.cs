using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ProgramExtensions
{
    /// <summary>
    /// Grid functions<br />
    /// 
    /// </summary>
    static public class ProgramExtensions
    {

        /* Assemblable items
        public const string Components = "BulletproofGlass,Computer,Construction,Detector,Display,Girder,GravityGenerator,InteriorPlate,LargeTube,Medical,MetalGrid,Motor,PowerCell,RadioCommunication,Reactor,SmallTube,SolarCell,SteelPlate,Thrust,Explosives";
        public const string Ammos = "Missile200mm,NATO_25x184mm,NATO_5p56x45mm";
        public const string Tools = "WelderItem,HandDrillItem,AngleGrinderItem,AutomaticRifleItem";
        public const string OxygenContainers = "OxygenBottle";
        */
        /// <summary>
        /// Checks if block is on same grid as currently running PB
        /// </summary>
        /// <param name="b">Block</param>
        /// <returns>True if block on this grid</returns>
        static public bool BelongsToGrid(this IMyTerminalBlock b, MyGridProgram gp)
        {
            return b.CubeGrid == gp.Me.CubeGrid;
        }

        static public IMyTerminalBlock GetBlock(this MyGridProgram gp, string name, bool onGrid = true)
        {
            var blocks = new List<IMyTerminalBlock>();

            if (onGrid)
                gp.GridTerminalSystem.SearchBlocksOfName(name, blocks, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            else
                gp.GridTerminalSystem.SearchBlocksOfName(name, blocks);
            if (blocks.Count == 0)
                return null;
            return blocks[0];
        }
        static public List<IMyTerminalBlock> SearchBlocks(this MyGridProgram gp, string name, bool onGrid = true)
        {
            var lst = new List<IMyTerminalBlock>();
            if (onGrid)
                gp.GridTerminalSystem.SearchBlocksOfName(name, lst, delegate(IMyTerminalBlock b) { return b.BelongsToGrid(gp); });
            else
                gp.GridTerminalSystem.SearchBlocksOfName(name, lst);
            return lst;
        }
        static public List<IMyTerminalBlock> GetBlockGroup(this MyGridProgram gp, string groupName)
        {
            List<IMyBlockGroup> grps = new List<IMyBlockGroup>();
            gp.GridTerminalSystem.GetBlockGroups(grps);
            var grp = grps.Find(x => x.Name.Contains(groupName));
            if (grp == null)
                return new List<IMyTerminalBlock>();
            else
                return grp.Blocks;
        }

    }
}
