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
        static public bool Debug = false;
        static List<string> debugSources = new List<string>();
        static public List<string> DebugSources = new List<string>();
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
            var group = groups.Find(x => { return x.Name.Contains(groupName); });
            if (group == null)
                return new List<IMyTerminalBlock>();
            else
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                group.GetBlocks(blocks);
                return blocks;
            }
            
        }
        static public bool OnGrid(this MyGridProgram gp, IMyTerminalBlock b)
        {
            return b.CubeGrid == gp.Me.CubeGrid;
        }
        static public void dbout(this MyGridProgram gp, string message, string source = "")
        {
            if (string.IsNullOrEmpty(source) || debugSources.Contains(source)) gp.Echo(message);
            if (ProgramExtensions.Debug && (source== "" || DebugSources.Contains(source))) gp.Echo(message);
        }
        static public void AddDebugSource(string source)
        {
            if (!DebugSources.Contains(source)) DebugSources.Add(source);
        }
      
           


    }

}
