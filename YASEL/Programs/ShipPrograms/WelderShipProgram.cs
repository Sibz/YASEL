using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace ShipManagerProgram
{

    using ShipManager;
    using ProgramExtensions;
    using InventoryExtensions;
    using TextPanelExtensions;
    
    partial class WelderShipProgram : MyGridProgram
    {
        string LCDLevelsName = "LCD Welder Component Levels",
            LCDTargetsName = "LCD Welder Component Targets",
            CargoWelderName = "Welder Cargo",
            CargoStationName = "Component Cargo";
        ShipManager sm;
        List<IMyInventory> WelderInvs = new List<IMyInventory>(), StationInvs = new List<IMyInventory>();
        IMyTextPanel LCDLevels, LCDTargets;
        void Main(string argument)
        {
            if (sm == null)
            {
                sm = new ShipManager(this);
                WelderInvs = this.GetBlockGroup(CargoWelderName).GetInventories();
                LCDTargets = this.GetBlock(LCDTargetsName) as IMyTextPanel;
                LCDLevels = this.GetBlock(LCDLevelsName) as IMyTextPanel;
            }
            StationInvs = this.GetBlockGroup(CargoStationName).GetInventories();

            if (argument == "")
            {
                sm.ManageDockingState();
                ManageStock();
                CountStock();
            }
            else if (argument == "Dock" || argument == "dock")
                sm.Dock();
        }

        void ManageStock()
        {
            if (!sm.IsDocked)
                return;

            var targets = LCDTargets.GetValueList();

            foreach (var target in targets)
            {
                var currentAmount = WelderInvs.CountItems(target.Key);
                if (currentAmount < target.Value)
                    StationInvs.MoveItemAmount(WelderInvs, target.Key, target.Value - currentAmount);
                else if (currentAmount > target.Value)
                    WelderInvs.MoveItemAmount(StationInvs, target.Key, currentAmount - target.Value);
            }

        }

        void CountStock()
        {
            var targets = LCDTargets.GetValueList();
            LCDLevels.WritePublicText("Stock Levels:\n", false);
            foreach (var target in targets)
            {
                var currentAmount = WelderInvs.CountItems(target.Key);
                if (currentAmount < target.Value * 0.75)
                    LCDLevels.WritePublicText(target.Key + ": " + currentAmount + "\n", true);
            }
        }
    }
}