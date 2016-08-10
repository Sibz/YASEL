using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;


namespace AssemblerManager
{
    using TextPanelExtensions;
    using ProgramExtensions;
    using InventoryExtensions;
    using BlockExtensions;

    class AssemblerManager
    {
        MyGridProgram gp;
        AssemblerManagerSettings m_settings;
        List<IMyTerminalBlock> m_assemblers;
        List<IMyTerminalBlock> m_cargo;
        Dictionary<string, double> m_stockLevels;

        public AssemblerManager(MyGridProgram gp, AssemblerManagerSettings settings)
        {
            this.gp = gp;
            m_settings = settings;

            m_assemblers = new List<IMyTerminalBlock>();
            m_cargo = new List<IMyTerminalBlock>();
            m_stockLevels = new Dictionary<string, double>();

            if (m_settings.LCDStockLevelsName == "")
                throw new Exception("No LCD specified for item levels. Set LCDStockLevelNames in settings.");
            if ((gp.GetBlock(m_settings.LCDStockLevelsName) as IMyTextPanel)==null)
                throw new Exception("Unable to access LCD with name provided. Check Name, that block exists and ownership is same as programmable block.");

            if (m_settings.AssemblerGroupName == "")
                gp.GridTerminalSystem.GetBlocksOfType<IMyAssembler>(m_assemblers, gp.OnGrid);
            else
                m_assemblers = gp.GetBlockGroup(m_settings.AssemblerGroupName);

            if (m_settings.CargoGroupName == "")
            {
                throw new Exception("CargoGroupName is required.");
            }
            else
                m_cargo = gp.GetBlockGroup(m_settings.CargoGroupName);
            if (m_cargo.Count == 0)
                throw new Exception("No blocks found in cargo group.");
        }
        public void Tick()
        {
            var tp = gp.GetBlock(m_settings.LCDStockLevelsName) as IMyTextPanel;

            m_stockLevels = tp.GetValueList();
            var stockEnum = m_stockLevels.GetEnumerator();
            while (stockEnum.MoveNext())
            {
                if (m_cargo.GetInventories().CountItems(stockEnum.Current.Key) < stockEnum.Current.Value)
                    getItemAssemblers(stockEnum.Current.Key).TurnOn();
                else
                    getItemAssemblers(stockEnum.Current.Key).TurnOff();
            }
        }

        List<IMyTerminalBlock> getItemAssemblers(string itemName)
        {
            var itemAssemblers = new List<IMyTerminalBlock>();
            m_assemblers.ForEach(assembler =>
            {
                if (assembler is IMyAssembler && assembler.CustomName.Contains(itemName))
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