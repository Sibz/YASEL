using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace ProgramExtensions
{
    /// <summary>
    /// Program Extensions<br />
    /// 
    /// </summary>
    static public class ProgramExtensions
    {

        static public IMyTerminalBlock GetBlock(this MyGridProgram gp, string name, bool onGrid = true)
        {
            var blocks = gp.SearchBlocks(name, onGrid);

            if (blocks.Count == 0)
                return null;
            return blocks[0];
        }
        static public List<IMyTerminalBlock> SearchBlocks(this MyGridProgram gp, string name, bool onGrid = true)
        {
            var blocks = new List<IMyTerminalBlock>();
            if (onGrid)
                gp.GridTerminalSystem.SearchBlocksOfName(name, blocks, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.SearchBlocksOfName(name, blocks);
            return blocks;
        }
        static public List<IMyTerminalBlock> GetBlockGroup(this MyGridProgram gp, string groupName)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            gp.GridTerminalSystem.GetBlockGroups(groups);
            var group = groups.Find(x => x.Name.Contains(groupName));
            if (group == null)
                return new List<IMyTerminalBlock>();
            else
                return group.Blocks;
            
        }
        
    }
}
