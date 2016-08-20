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
        public ReactorInfoModule(MyGridProgram gp, int id = -1) : base(gp, id)
        {
            defaultArgs.Add("display", "count;output;fuel;reactors;reactorName;reactorState;reactorOutput;reactorFuel");
            defaultArgs.Add("countPrefix", "Count: ");
            defaultArgs.Add("outputPrefix", "Power output: ");
            defaultArgs.Add("fuelPrefix", "Uranium: ");
            defaultArgs.Add("reactorNamePrefix", " ");
            defaultArgs.Add("reactorStatePrefix", "\n  Status: ");
            defaultArgs.Add("reactorOutputPrefix", "\n  Power output: ");
            defaultArgs.Add("reactorFuelPrefix", "\n  Uranium: ");
            defaultArgs.Add("outputType", "power");
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
            setValue("reactors", reactorsString + "\n");

        }
    }
}
