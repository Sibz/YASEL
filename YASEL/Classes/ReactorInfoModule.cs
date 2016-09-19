using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;


namespace ReactorInfoModule
{

    using StatusDisplay;
    using ProgramExtensions;
    using InventoryExtensions;
    using BlockExtensions;
    class ReactorInfoModule : StatusDisplayModule
    {
        Dictionary<int, FuelReadings> instanceReadings = new Dictionary<int, FuelReadings>();
        public ReactorInfoModule(MyGridProgram gp, Dictionary<string, string> defaultArgs = null, int id = -1) : base(gp, defaultArgs, id)
        {
            addValueDefinition("count", "Count: ");
            addValueDefinition("output", "Power output: ", "power");
            addValueDefinition("fuel", "Uranium: ");
            addValueDefinition("fuelRate", "Uranium used per hour: ");
            addValueDefinition("reactors");
            addValueDefinition("reactorName", " ");
            addValueDefinition("reactorState", "\n" + getDefaultArg("pad") + "  Status: ");
            addValueDefinition("reactorOutput", "\n" + getDefaultArg("pad") + "  Power output: ");
            addValueDefinition("reactorFuel", "\n" + getDefaultArg("pad") + "  Uranium: ");
        }

        internal override string commandName
        {
            get
            {
                return "reactorInfo";
            }
        }

        internal override void update()
        {

            // get reactors
            var groupName = getArg("group");
            var reactors = new List<IMyTerminalBlock>();
            if (groupName == "#all#")
            {
                Func<IMyTerminalBlock, bool> collect = null;
                if (getArgBool("onGrid")) collect = gp.OnGrid;
                gp.GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors, collect);
            }
            else
            {
                reactors = gp.GetBlockGroup(groupName);
            }

            float output = 0f, fuel = 0f;

            string reactorsString = "";
            foreach (IMyReactor reactor in reactors)
            {
                output += reactor.CurrentOutput;
                var fuelInvs = reactor.GetInventory(0);
                var curFuel = 0f;
                fuel += curFuel = fuelInvs.CountItems("Uranium");
                var reactorArgs = getArg("display").Split(';');

                foreach (string reactorArg in reactorArgs)
                {
                    switch (reactorArg)
                    {
                        case "reactorName":
                            reactorsString += getArg("reactorNamePrefix") + reactor.CustomName;
                            break;
                        case "reactorState":
                            reactorsString += getArg("reactorStatePrefix") + (reactor.IsEnabled() ? "On" : "Off");
                            break;
                        case "reactorFuel":
                            reactorsString += getArg("reactorFuelPrefix") + Math.Round(curFuel, 2);
                            break;
                        case "reactorOutput":
                            reactorsString += getArg("reactorOutputPrefix") + getValuePower("", reactor.CurrentOutput);
                            break;
                    }
                }

            }
            setValueInt("count", reactors.Count);
            setValueFloat("output", output);
            setValueFloat("fuel", (float)Math.Round(fuel, 2));
            if (!instanceReadings.ContainsKey(currentInstanceId))
                instanceReadings.Add(currentInstanceId, new FuelReadings(fuel));
            instanceReadings[currentInstanceId].AddReading(fuel);
            setValueFloat("fuelRate", (float)Math.Round(instanceReadings[currentInstanceId].GetAvg(),2));
            setValue("reactors", reactorsString);

        }
        class FuelReadings
        {
            Queue<KeyValuePair<DateTime, float>> readings = new Queue<KeyValuePair<DateTime, float>>();
            KeyValuePair<DateTime, float> lastReading = new KeyValuePair<DateTime, float>();
            float fuel;
            bool firstReading = true;
            public FuelReadings(float startingFuel)
            {
                fuel = startingFuel;
            }
            public void AddReading(float reading)
            {
                if (lastReading.Key < DateTime.Now.AddMinutes(-10))
                {
                    lastReading = new KeyValuePair<DateTime, float>(DateTime.Now, fuel-reading);
                    fuel = reading;
                    readings.Enqueue(lastReading);
                    if (firstReading == true && readings.Count>1)
                    {
                        firstReading = false;
                        readings.Dequeue();
                    }
                }
                if (readings.Count > 6*12)
                    readings.Dequeue();
            }
            public float LastReading { get { return lastReading.Value; } }
            public float GetAvg()
            {
                float result = 0f;
                foreach (var r in readings)
                {
                    result += r.Value;
                }
                return result / readings.Count*6;
            }
        }
    }
}
