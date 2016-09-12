using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RefineryManagerProgram
{

    using RefineryManager;
    using ProgramExtensions;
    using InventoryExtensions;

    class RefineryManagerProgram : MyGridProgram
    {
        RefineryManager refineryManager;
        List<IMyTerminalBlock> fromCargo, toCargo, toCargoEff;
        List<string> prodOres = new List<string>()
            {
                "Iron",
                "Cobalt",
                "Nickel",
                "Silicon",
                "Silver",
                "Stone"
            },
            effOres = new List<string>()
            {
                "Uranium",
                "Gold",
                "Platinum",
                "Magnesium"
            };
        void Main(string argument)
        {

            if (refineryManager == null)
            {
                refineryManager = new RefineryManager(this);
                refineryManager.AddRefineryGroup("Prod Ore", "Productive Refineries");
                refineryManager.AddRefineryGroup("Eff Ore", "Effective Refineries");
                fromCargo = this.GetBlockGroup("Ore Cargo");
                toCargo = this.GetBlockGroup("Prod Ore");
                toCargoEff = this.GetBlockGroup("Eff Ore");
            }
            foreach (var ore in prodOres)
                fromCargo.GetInventories().MoveItemAmount(toCargo.GetInventories(), ore, new VRage.MyFixedPoint(), "Ore");
            foreach (var ore in effOres)
                fromCargo.GetInventories().MoveItemAmount(toCargoEff.GetInventories(), ore, new VRage.MyFixedPoint(), "Ore");

            refineryManager.ManageRefineries();
        }

    }
}