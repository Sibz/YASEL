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

    partial class DrillShipProgram : MyGridProgram
    {
        string
            CargoStationName = "Incoming Ore";
        
        ShipManager sm;
        List<IMyInventory> DrillerInvs = new List<IMyInventory>(), StationInvs = new List<IMyInventory>();
        void Main(string argument)
        {
            if (sm == null)
            {
                sm = new ShipManager(this);
                DrillerInvs = this.GetInventories();
            }
            StationInvs = this.GetBlockGroup(CargoStationName).GetInventories();

            if (argument == "")
            {
                sm.ManageDockingState();
                ManageStock();
            }
            else if (argument == "Dock" || argument == "dock")
                sm.Dock();
        }

        void ManageStock()
        {
            if (!sm.IsDocked)
                return;

            DrillerInvs.MoveItemAmount(StationInvs,"", (double)0, "Ore");

        }


    }
}