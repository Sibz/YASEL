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

    public class BatteryInfoModule : StatusDisplayModule
    {
        private string groupName = "#all#";
        private Dictionary<int, avgDiff> instanceDiffs = new Dictionary<int, avgDiff>();
        public BatteryInfoModule(MyGridProgram gp, int id = -1) : base(gp, id)
        {
            defaultArgs.Add("display", "count;percent;input;output;maxStored;stored;time");

            defaultArgs.Add("countPrefix", "Quantity: ");
            defaultArgs.Add("percentPrefix", "Total charge: ");
            defaultArgs.Add("inputPrefix", "Input: ");
            defaultArgs.Add("outputPrefix", "Output: ");
            defaultArgs.Add("maxStoredPrefix", "Max Storage: ");
            defaultArgs.Add("storedPrefix", "Power stored: ");
            defaultArgs.Add("timeToChargePrefix", "Charged in: ");
            defaultArgs.Add("timeToDischargePrefix", "Depleted in: ");
            defaultArgs.Add("fullyChargedPrefix", "Fully Charged");

            defaultArgs.Add("percentType", "percent");
            defaultArgs.Add("inputType", "power");
            defaultArgs.Add("outputType", "power");
            defaultArgs.Add("maxStoredType", "power");
            defaultArgs.Add("storedType", "power");
            defaultArgs.Add("timeToChargeType", "time");
            defaultArgs.Add("timeToDischargeType", "time");

            defaultArgs.Add("chargeRounding", "1");

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
        private string timeToCharge()
        {
            var percent = getValueFloat("stored") / getValueFloat("maxStored");
            var lastPercent = getValueFloat("lastPercent");
            setValueFloat("lastPercent", percent);
            if (Math.Round(percent * 100, getArgInt("chargeRounding")) == 100)
                return "Fully Charged";
            bool charging = lastPercent < percent;
            gp.dbout("LastPercent" + lastPercent + "\nvs\vChargePercent" + percent);
            gp.dbout("Updated LastPercen=" + getValueFloat("lastPercent"));
            float chargeDiff = (charging ? percent - lastPercent : lastPercent - percent);
            if (!instanceDiffs.ContainsKey(currentInstanceId))
                instanceDiffs.Add(currentInstanceId, new avgDiff());
            instanceDiffs[currentInstanceId].addDiff(chargeDiff);
            var runsTillCharged = (charging ? 1 - (percent) : percent) / instanceDiffs[currentInstanceId].avg();
            double msTillCharged = gp.Runtime.TimeSinceLastRun.TotalMilliseconds * runsTillCharged;
            TimeSpan chargeTime = new TimeSpan(0, 0, 0, 0, (int)msTillCharged);
            return (charging ? "Charged in" : "Depleted in") + ": "
                + (chargeTime.Hours > 0 ? chargeTime.Hours + "Hr" + (chargeTime.Hours > 1 ? "s " : " ") : " ") + chargeTime.Minutes + "min\n";
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