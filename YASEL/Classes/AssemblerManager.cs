using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AssemblerManager
{
    using Grid;
    using Str;
    using TextPanel;
    using Inventory;
    using Block;
    class AssemblerManager
    {
        AssemblerManagerSettings m_settings;
        List<IMyTerminalBlock> m_assemblers;
        List<IMyTerminalBlock> m_cargo;
        Dictionary<string, double> m_stockLevels;

        public AssemblerManager(AssemblerManagerSettings settings)
        {
            m_settings = settings;

            m_assemblers = new List<IMyTerminalBlock>();
            m_cargo = new List<IMyTerminalBlock>();
            m_stockLevels = new Dictionary<string, double>();

            if (m_settings.LCDStockLevelsName == "")
                throw new Exception("No LCD specified for item levels. Set LCDStockLevelNames in settings.");
            if ((Grid.GetBlock(m_settings.LCDStockLevelsName) as IMyTextPanel)==null)
                throw new Exception("Unable to access LCD with name provided. Check Name, that block exists and ownership is same as programmable block.");

            if (m_settings.AssemblerGroupName == "")
                Grid.ts.GetBlocksOfType<IMyAssembler>(m_assemblers, Grid.BelongsToGrid);
            else
                m_assemblers = Grid.GetBlockGrp(m_settings.AssemblerGroupName);

            if (m_settings.CargoGroupName == "")
                Grid.ts.GetBlocksOfType<IMyInventoryOwner>(m_cargo, Grid.BelongsToGrid);
            else
                m_cargo = Grid.GetBlockGrp(m_settings.CargoGroupName);

        }
        public void Tick()
        {
            m_stockLevels = TextPanel.GetValueListFromLCD(m_settings.LCDStockLevelsName);
            var stockEnum = m_stockLevels.GetEnumerator();
            while (stockEnum.MoveNext())
            {
                if (Inventory.CountItems(m_cargo, "", stockEnum.Current.Key) < stockEnum.Current.Value)
                    Block.TurnOnOff(getItemAssemblers(stockEnum.Current.Key));
                else
                    Block.TurnOnOff(getItemAssemblers(stockEnum.Current.Key), false);
            }
        }

        List<IMyTerminalBlock> getItemAssemblers(string itemName)
        {
            var itemAssemblers = new List<IMyTerminalBlock>();
            m_assemblers.ForEach(assembler =>
            {
                if (assembler is IMyAssembler && Str.Contains(assembler.CustomName,itemName))
                    itemAssemblers.Add(assembler);
            });
            return itemAssemblers;
        }
    }

    class AssemblerManagerSettings
    {
        public string AssemblerGroupName = "";
        public string CargoGroupName = "";
        public string LCDStockLevelsName = "";
    }
}