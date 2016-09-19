using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

using VRage.Game.ModAPI.Ingame;

namespace MyStationProgram
{
    using StatusDisplay;
    using BatteryInfoModule;
    using ReactorInfoModule;
    using SolarInfoModule;
    using OreInfoModule;
    using ProgramExtensions;
    using TaskQueue;
    using RefineryManager;
    using InventoryExtensions;
    using AssemblerManager;
    class Program : MyGridProgram
    {
        TaskQueue queue;
        // Status display vars
        StatusDisplay statusDisplay;
        StatusDisplaySettings settings = new StatusDisplaySettings();

        // REfinery Vars:
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

        // Assembler manager vars
        Dictionary<string, double> targets = new Dictionary<string, double>();

        AssemblerManager myAM;

        public Program()
        {
            queue = new TaskQueue(this, this.GetBlock("Queue Timer") as IMyTimerBlock);
            queue.Enqueue(initStatusDisplay);
            queue.Enqueue(processRefineryManager);
            queue.Enqueue(initAssemblerManager);
        }

        void Main(string argument)
        {
            queue.Tick();
            queue.Enqueue(processRefineryManager);
            queue.Enqueue(myAM.Tick);
        }

        void initStatusDisplay()
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("pad", "  ");

            settings.Modules.Add(new BatteryInfoModule(this, args));
            settings.Modules.Add(new ReactorInfoModule(this, args));
            settings.Modules.Add(new SolarInfoModule(this, args));
            settings.Modules.Add(new OreInfoModule(this, args));

            statusDisplay = new StatusDisplay(this, ref queue, settings);
        }
        void processRefineryManager()
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
        void initAssemblerManager()
        {
            if (myAM == null)
            {
                targets.Add("SteelPlate", 5000);
                targets.Add("InteriorPlate", 2500);
                targets.Add("Construction", 5000);
                targets.Add("LargeTube", 500);
                targets.Add("SmallTube", 1500);
                targets.Add("Motor", 1500);
                targets.Add("Computer", 2000);
                targets.Add("MetalGrid", 1000);
                targets.Add("BulletproofGlass", 2000);
                targets.Add("Display", 1000);
                targets.Add("Girder", 1000);
                //targets.Add("", 0);
                //targets.Add("", 0);


                // settings
                AssemblerManagerSettings myAM_Settings = new AssemblerManagerSettings
                {
                    // if you want to only use some assemblers in checks uncomment line below and provide group name
                    // AssemblerGroupName = "", 

                    // REQUIRED - provide a group of cargo containers to count items from, otherwise it counts from all inventories
                    CargoGroupName = "Component Storage",
                    IngotStorageName = "Ingot Storage",

                    // REQUIRED, LCD with list of items and stock levels 
                    // OR - use Targets Dictionary of string/double:
                    //LCDStockLevelsName = ""
                    /*
                     Example List:
                      SteelPlate:1000
                      MetalGrid:500
                      Construction:1500
                     */
                    Targets = targets
                };
                myAM = new AssemblerManager(this, myAM_Settings);
            }
        }
    }

}
