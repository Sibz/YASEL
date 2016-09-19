using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace TestProgram
{
    using ProgramExtensions;
    using Graph;
    using TextPanelExtensions;
    using InventoryExtensions;

    class IngotLevelsProgram : MyGridProgram
    {
        IMyTerminalBlock lcdListTargetValues;
        List<IMyTerminalBlock> lcdsGraph = new List<IMyTerminalBlock>();
        List<IMyInventory> ingotCargo;
        Dictionary<string, double> currentValues = new Dictionary<string, double>();
       
        void Main(string argument)
        {
            if (lcdListTargetValues == null)
            {
                lcdListTargetValues = this.GetBlock("LCD Ingot Targets", true);
                if (lcdListTargetValues == null)
                    throw new Exception("Unable to load LCD with target values");
            }
            if (lcdsGraph.Count == 0)
            {
                lcdsGraph = this.SearchBlocks("LCD Ingot Levels", true);
                if (lcdsGraph.Count == 0)
                    throw new Exception("Unable to load LCDs for graph");
            }
            if (ingotCargo == null)
            {
                ingotCargo = this.SearchBlocks("Cargo Ingots").GetInventories();
                if (ingotCargo == null || ingotCargo.Count == 0)
                    throw new Exception("Unable to get cargo inventories");
            }
            var targetValues = (lcdListTargetValues as IMyTextPanel).GetValueList();
            var tvEnum = targetValues.GetEnumerator();
            while(tvEnum.MoveNext())
            {
                if (currentValues.ContainsKey(tvEnum.Current.Key))
                    currentValues[tvEnum.Current.Key] = ingotCargo.CountItems(tvEnum.Current.Key);
                else
                    currentValues.Add(tvEnum.Current.Key, ingotCargo.CountItems(tvEnum.Current.Key));
            }
            string graph = Graph.PrepareBarGraph(targetValues, currentValues, 0.43);
            lcdsGraph.WriteToScreens(graph);
        }

    }

}