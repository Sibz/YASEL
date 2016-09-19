using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

using VRage.Game.ModAPI.Ingame;

namespace RefineryManagerProgram
{

    using RefineryManager;
    using ProgramExtensions;
    using InventoryExtensions;
    class RefineryManagerProgram : MyGridProgram
    {
        string IngotCargoName = "Ingot Storage";
        RefineryManager refineryManager;
        List<IMyTerminalBlock> fromCargo, toCargo, toCargoEff;
        List<IMyInventory> ingotInvs, refIngotInvs;
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

            refineryManager = new RefineryManager(this);
            refineryManager.AddRefineryGroup("Prod Ore", "Productive Refineries");
            refineryManager.AddRefineryGroup("Eff Ore", "Effective Refineries");
            fromCargo = this.GetBlockGroup("Incoming Ore");
            toCargo = this.GetBlockGroup("Prod Ore");
            toCargoEff = this.GetBlockGroup("Eff Ore");
            refIngotInvs = this.GetBlockGroup("Productive Refineries").GetInventories(1);
            refIngotInvs.AddList(this.GetBlockGroup("Effective Refineries").GetInventories(1));
            ingotInvs = this.GetBlockGroup(IngotCargoName).GetInventories();
            Echo("ingotInvs = " + ingotInvs.Count + " refInvs = " + refIngotInvs.Count);
            foreach (var ore in prodOres)
                fromCargo.GetInventories().MoveItemAmount(toCargo.GetInventories(), ore, (double)0, "Ore");
            foreach (var ore in effOres)
                fromCargo.GetInventories().MoveItemAmount(toCargoEff.GetInventories(), ore, (double)0, "Ore");
            foreach (var inv in refIngotInvs)
                refIngotInvs.MoveItemAmount(ingotInvs, "", (double)0, "Ingot", 0.98f, this);
            refineryManager.ManageRefineries();
        }

    }
}