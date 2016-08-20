using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace SolarInfoModule
{
    using ProgramExtensions;
    using StatusDisplay;
    class SolarInfoModule : StatusDisplayModule
    {
        public SolarInfoModule(MyGridProgram gp, Dictionary<string,string> defaultArgs = null, int id = -1) : base(gp, defaultArgs, id)
        {
            addValueDefinition("count", "Count: ");
            addValueDefinition("maxPower", "Max output: ", "power");
            addValueDefinition("currentPower", "Current output: ", "power");
            setDefaultArgBool("onGrid", false);
        }

        internal override string commandName
        {
            get
            {
                return "solarInfo";
            }
        }

        internal override void update()
        {
            var groupName = getArg("group");

            var panels = new List<IMyTerminalBlock>();
            if (groupName == "#all#")
            {
                Func<IMyTerminalBlock, bool> collect = null;
                if (getArgBool("onGrid")) collect = gp.OnGrid;
                gp.GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(panels, collect);
            }
            else
            {
                panels = gp.GetBlockGroup(groupName);
            }

            float max = 0f, current = 0f;
            foreach(var panel in panels)
            {
                if (panel is IMySolarPanel)
                {
                    max += ((IMySolarPanel)panel).MaxOutput;
                    current += ((IMySolarPanel)panel).MaxOutput;
                }
            }
            setValueInt("count", panels.Count);
            setValueFloat("maxPower", max);
            setValueFloat("currentPower", current);
        }
    }
}
