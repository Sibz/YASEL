using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace OreInfoModule
{

    using StatusDisplay;
    using ProgramExtensions;
    using InventoryExtensions;
    using BlockExtensions;
    using Graph;

    class OreInfoModule : StatusDisplayModule
    {
        List<string> Elements = new List<string>() {
            "Iron",
            "Silicon",
            "Cobalt",
            "Nickel",
            "Silver",
            "Gold",
            "Uranium",
            "Platinum",
            "Magnesium"
        };
        public OreInfoModule(MyGridProgram gp, Dictionary<string, string> defaultArgs = null, int id = -1) : base(gp, defaultArgs, id)
        {
            addValueDefinition("listOre", "");
            addValueDefinition("listIngots", "");
            addValueDefinition("oreCargoCapacity", "Cargo Capacity: ", "percent", false);
            addValueDefinition("oreRate", "Ore rate: ", "", false);
            setDefaultArg("oreGroup", "none");
            setDefaultArg("maxIron", "500000");
            setDefaultArg("maxSilicon", "500000");
            setDefaultArg("maxNickel", "200000");
            setDefaultArg("maxCobalt", "175000");
            setDefaultArg("maxSilver", "50000");
            setDefaultArg("maxGold", "10000");
            setDefaultArg("maxUranium", "1000");
            setDefaultArg("maxPlatinum", "500");
            setDefaultArg("maxMagnesium", "100");

        }

        internal override string commandName
        {
            get
            {
                return "oreInfo";
            }
        }

        internal override void update()
        {
            Dictionary<string, double> oreTargets = new Dictionary<string, double>(), ingotTargets = new Dictionary<string, double>();

            // get cargo to count from
            var groupName = getArg("group");
            var countCargo = new List<IMyTerminalBlock>();
            if (groupName == "#all#")
            {
                Func<IMyTerminalBlock, bool> collect = null;
                if (getArgBool("onGrid")) collect = gp.OnGrid;
                gp.GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(countCargo, collect);
            }
            else
            {
                countCargo = gp.GetBlockGroup(groupName);
            }

            // just ore cargo
            if (getArg("oreGroup") != "none")
            {
                var oreCargo = gp.GetBlockGroup(getArg("oreGroup")).GetInventories();
                double max = 1f, cur = 0f;
                foreach (var inv in oreCargo)
                {
                    max += (double)inv.MaxVolume;
                    cur += (double)inv.CurrentVolume;
                }
                setValueFloat("oreCargoCapacity", (float)(cur / max));
            }


            Dictionary<string, double> oreCounts = new Dictionary<string, double>(), ingotCounts = new Dictionary<string, double>();

            foreach (var element in Elements)
            {
                var targetIngots = getArgFloat("max" + element);
                ingotTargets.Add(element, targetIngots);



                var ingotLevel = countCargo.GetInventories().CountItems(element, "Ingot");
                ingotCounts.Add(element, ingotLevel);

                if (ingotLevel > targetIngots)
                {
                    oreCounts.Add(element, 1);
                    oreTargets.Add(element, 1);
                }
                else
                {
                    oreCounts.Add(element, countCargo.GetInventories().CountItems(element, "Ore"));
                    oreTargets.Add(element, ingotToOre(element, targetIngots) - ingotToOre(element, ingotLevel));
                }
            }

            setValue("listOre", Graph.PrepareBarGraph(oreTargets, oreCounts));
            setValue("listIngots", Graph.PrepareBarGraph(ingotTargets, ingotCounts));

        }
        private float ingotToOre(string type, float value)
        {
            switch (type)
            {
                case "Stone":
                    return value / 0.9f;
                case "Iron":
                    return value / 0.7f;
                case "Silicon":
                    return value / 0.7f;
                case "Nickel":
                    return value / 0.4f;
                case "Cobalt":
                    return value / 0.3f;
                case "Silver":
                    return value / 0.1f;
                case "Gold":
                    return value / 0.01f;
                case "Uranium":
                    return value / 0.007f;
                case "Magnesium":
                    return value / 0.007f;
                case "Platinum":
                    return value / 0.005f;
            }
            throw new Exception("ingotToOre called on bad type: " + type);
        }
    }
}
