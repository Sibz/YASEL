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
            "Stone",
            "Iron",
            "Silicon",
            "Cobalt",
            "Nickel",
            "Silver",
            "Gold",
            "Uranium",
            //"Platinum",
            "Magnesium"
        };
        Dictionary<string, Dictionary<int, timeLeftCalculator>> timeCalcs = new  Dictionary<string, Dictionary<int, timeLeftCalculator>>();
        public OreInfoModule(MyGridProgram gp, Dictionary<string, string> defaultArgs = null, int id = -1) : base(gp, defaultArgs, id)
        {
            addValueDefinition("listElements", "", "", true);
            addValueDefinition("listOre", "", "", false);
            addValueDefinition("listIngots", "", "", false);
            addValueDefinition("oreCargoCapacity", "Cargo Capacity: ", "percent", true);
            addValueDefinition("oreRate", "Ore rate: ", "time", true);
            foreach (var element in Elements)
                addValueDefinition(element + "Rate", element+ " rate: ", "time", false);
            setDefaultArg("oreGroup", "none");
            setDefaultArg("maxIron", "500000");
            setDefaultArg("maxSilicon", "500000");
            setDefaultArg("maxStone", "100000");
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
            float percent = 0f;
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
            var oreCargo = new List<IMyInventory>();
            // just ore cargo
            if (getArg("oreGroup") != "none")
            {
                oreCargo = gp.GetBlockGroup(getArg("oreGroup")).GetInventories();
                double max = 1f, cur = 0f;
                foreach (var inv in oreCargo)
                {
                    max += (double)inv.MaxVolume;
                    cur += (double)inv.CurrentVolume;
                }
                percent = (float)(cur / max);
            }
            setValueFloat("oreCargoCapacity", percent);



            Dictionary<string, double> oreCounts = new Dictionary<string, double>(), ingotCounts = new Dictionary<string, double>(), elementCounts = new Dictionary<string, double>();
            TimeSpan oreRate = new TimeSpan();
            foreach (var element in Elements)
            {
                var targetIngots = getArgFloat("max" + element);
                ingotTargets.Add(element, targetIngots);

               


                var ingotLevel = countCargo.GetInventories().CountItems(element, "Ingot");
                ingotCounts.Add(element, ingotLevel);
                var oreLevel = oreCargo.CountItems(element, "Ore");

                if (ingotLevel > targetIngots)
                {
                    oreCounts.Add(element, 1);
                    oreTargets.Add(element, 1);
                }
                else
                {
                    oreCounts.Add(element, oreLevel);
                    oreTargets.Add(element, ingotToOre(element, targetIngots) - ingotToOre(element, ingotLevel));
                    elementCounts.Add(element, oreToIngot(element, oreLevel) + ingotLevel);
                }

                //


                if (!timeCalcs.ContainsKey(element))
                    timeCalcs.Add(element, new Dictionary<int, timeLeftCalculator>());


                if (!timeCalcs[element].ContainsKey(currentInstanceId))
                    timeCalcs[element].Add(currentInstanceId, new timeLeftCalculator());
                timeCalcs[element][currentInstanceId].addPercent(oreLevel);
                var time = timeCalcs[element][currentInstanceId].getTime(gp.Runtime.TimeSinceLastRun.TotalMilliseconds, oreLevel);
                setValueInt(element + "Rate", time);
                oreRate=  oreRate.Add(new TimeSpan(0, 0, 0, 0, time));
            }

            setValueInt("oreRate", (int)oreRate.TotalMilliseconds);

            setValue("listOre", Graph.PrepareBarGraph(oreTargets, oreCounts, 0.54, "curLevel"));
            setValue("listIngots", Graph.PrepareBarGraph(ingotTargets, ingotCounts));
            setValue("listElements", Graph.PrepareBarGraph(ingotTargets, elementCounts));

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
        private float oreToIngot(string type, float value)
        {
            switch (type)
            {
                case "Stone":
                    return value * 0.9f;
                case "Iron":
                    return value * 0.7f;
                case "Silicon":
                    return value * 0.7f;
                case "Nickel":
                    return value * 0.4f;
                case "Cobalt":
                    return value * 0.3f;
                case "Silver":
                    return value * 0.1f;
                case "Gold":
                    return value * 0.01f;
                case "Uranium":
                    return value * 0.007f;
                case "Magnesium":
                    return value * 0.007f;
                case "Platinum":
                    return value * 0.005f;
            }
            throw new Exception("ingotToOre called on bad type: " + type);
        }
        public class timeLeftCalculator
        {
            Queue<float> diffs = new Queue<float>();
            float lastPercent = 0f;
            bool firstValue = true;

            public void addDiff(float d)
            {
                diffs.Enqueue(d);
                if (diffs.Count > 300)
                    diffs.Dequeue();
            }
            public void addPercent(float percent)
            {
                if (!firstValue)
                    addDiff(lastPercent - percent);
                firstValue = false;
                lastPercent = percent;
            }
            public float avg()
            {
                float theAvg = 0f;
                foreach (float f in diffs)
                {
                    theAvg += f;
                }
                return theAvg / diffs.Count;
            }
            public int getTime(double TotalMilliseconds, float level = -1f)
            {
                if (avg() == 0)
                    return 0;
                return (int)(TotalMilliseconds * ((level == -1f ? lastPercent: level) / avg()));
            }
        }
    }
}
