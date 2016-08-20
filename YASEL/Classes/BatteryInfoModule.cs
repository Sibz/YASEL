using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;


namespace BatteryInfoModule
{
    using StatusDisplay;
    using ProgramExtensions;
    using BlockExtensions;

    public class BatteryInfoModule : StatusDisplayModule
    {
        private string groupName = "#all#";
        private Dictionary<int, avgDiff> instanceDiffs = new Dictionary<int, avgDiff>();
        public BatteryInfoModule(MyGridProgram gp, Dictionary<string,string> defaultArgs = null, int id = -1) : base(gp, defaultArgs, id)
        {
            addValueDefinition("count", "Quantity: ");
            addValueDefinition("percent", "Total charge: ", "percent");
            addValueDefinition("input", "Input:", "power");
            addValueDefinition("output", "Output:", "power");
            addValueDefinition("maxStored", "Max storage:", "power");
            addValueDefinition("stored", "Power stored:", "power");
            addValueDefinition("time");
            addValueDefinition("timeToCharge", "Charged in:", "time", false);
            addValueDefinition("timeToDischarge", "Depleted in:", "time", false);
            addValueDefinition("fullyCharged", "Fully Charged", "", false);


            setDefaultArg("chargeRounding", "1");
            setDefaultArg("switchReactors", "false");
            setDefaultArg("switchReactorsOn", "5");
            setDefaultArg("switchReactorsOff", "50");

        }
        internal override string commandName
        {
            get
            {
                return "batteryInfo";
            }
        }

        internal override void update()
        {
            gp.dbout(" - inside batteryInfo.execute");

            groupName = getArg("group");

            var batteries = new List<IMyTerminalBlock>();
            if (groupName == "#all#")
            {
                Func<IMyTerminalBlock, bool> collect = null;
                if (getArgBool("onGrid")) collect = gp.OnGrid;
                gp.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, collect);
            }
            else
            {
                batteries = gp.GetBlockGroup(groupName);
            }
            gp.dbout("Loaded batteries");
            float input = 0f, output = 0f, maxStored = 0f, stored = 0f;

            foreach (IMyBatteryBlock b in batteries)
            {
                input += b.CurrentInput;
                output += b.CurrentOutput;
                maxStored += b.MaxStoredPower;
                stored += b.CurrentStoredPower;
            }
            setValueFloat("input", input);
            setValueFloat("output", output);
            setValueFloat("maxStored", maxStored);
            setValueFloat("stored", stored);
            setValueInt("count", batteries.Count);
            var percent = stored / maxStored;
            var roundedPercent = (float)Math.Round(percent, getArgInt("chargeRounding") + 2);
            setValueFloat("percent", roundedPercent);
            var lastPercent = getValueFloat("lastPercent");
            setValueFloat("lastPercent", percent);
            bool charging = lastPercent < percent;
            setValueBool("charging", charging);
            gp.dbout("charging: " + charging);

            int chargeTime = getChargeTime(charging, percent, lastPercent);
            gp.dbout("Charging Time: " + chargeTime);

            if (charging)
                setValueInt("timeToCharge", chargeTime);
            else
                removeValue("timeToCharge");

            if (!charging)
                setValueInt("timeToDischarge", chargeTime);
            else
                removeValue("timeToDischarge");

            if (roundedPercent == 1)
                setValue("fullyCharged", "");
            else
                removeValue("fullyCharged");
            gp.dbout("roundedPercent:" + roundedPercent);
            //gp.dbout("timeToCharge:" + getTypedValue("timeToCharge"));
            //gp.dbout("timeToDischarge:" + getTypedValue("timeToDischarge"));
            setValue("time", roundedPercent == 1 ? getTypedValue("fullyCharged") :
                (charging ? getTypedValue("timeToCharge") : getTypedValue("timeToDischarge")));

            //Switch Reactors
            if (getArgBool("switchReactors"))
            {
                var reactors = new List<IMyTerminalBlock>();
                if (getArg("reactorName") != "")
                {
                    var block = gp.GetBlock(getArg("reactorName"));
                    if (block != null)
                    {
                        reactors.Add(block);
                    }
                } else if (getArg("reactorGroup") != "")
                {
                    reactors = gp.GetBlockGroup(getArg("reactorGroup"));
                } else
                    gp.GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors, gp.OnGrid);
                if (roundedPercent * 100 > getArgInt("switchReactorsOff"))
                    reactors.TurnOff();
                else if (roundedPercent * 100 < getArgInt("switchReactorsOn"))
                    reactors.TurnOn();
            }
        }
        private int getChargeTime(bool charging, float percent, float lastPercent)
        {
            float chargeDiff = (charging ? percent - lastPercent : lastPercent - percent);
            if (!instanceDiffs.ContainsKey(currentInstanceId))
                instanceDiffs.Add(currentInstanceId, new avgDiff());
            instanceDiffs[currentInstanceId].addDiff(chargeDiff);
            var runsTillCharged = (charging ? 1 - (percent) : percent) / instanceDiffs[currentInstanceId].avg();
            return (int)(gp.Runtime.TimeSinceLastRun.TotalMilliseconds * runsTillCharged);
        }

    }
    public class avgDiff
    {
        Queue<float> diffs = new Queue<float>();

        public void addDiff(float d)
        {
            diffs.Enqueue(d);
            if (diffs.Count > 10)
                diffs.Dequeue();
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
    }
}