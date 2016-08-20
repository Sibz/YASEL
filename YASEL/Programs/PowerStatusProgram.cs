// standard using statments
using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using SpaceEngineers.Game.ModAPI.Ingame;

// Wrap your program in a custom namespace, This has to be your file name
namespace ExampleProgram
{
    //using SolarExtensions;
    using ProgramExtensions;
    using BatteryExtensions;
    using BlockExtensions;
    using TextPanelExtensions;
       // Your programs class, must extend MyGridProgram, otherwise YASEL Exporter won't work.
    class SolarStatusProgram : MyGridProgram
    {
        List<IMyTerminalBlock> panels = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> batteries = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> reactors = new List<IMyTerminalBlock>();
        List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>();
        float lastChargePercent = 0f;
        avgDiff avgDiffs = new avgDiff();
        void Main(string argument)
        {
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, this.OnGrid);
            GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(panels);
            GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors, this.OnGrid);
            lcds = this.SearchBlocks("LCD Power");
            double maxPower = 0;
            double curPower = 0;
            float chargePercent = batteries.ChargePercent();
            bool charging = lastChargePercent < chargePercent;
            float chargeDiff = (charging ? chargePercent - lastChargePercent : lastChargePercent - chargePercent);
            Echo("chargeDiff:" + chargeDiff);
            avgDiffs.addDiff(chargeDiff);
            lastChargePercent = chargePercent;
            var runsTillCharged = (charging ? 1 - (chargePercent) : chargePercent) / avgDiffs.avg();
            double msTillCharged = Runtime.TimeSinceLastRun.TotalMilliseconds * runsTillCharged;
            Echo("msTillCharged:" + msTillCharged);
            TimeSpan chargeTime = new TimeSpan(0,0,0,0,(int)msTillCharged);
            Echo("chargeTime:" + chargeTime.TotalMilliseconds);
            if (chargePercent < 0.05)
                reactors.TurnOn();
            else if (chargePercent > 0.50)
                reactors.TurnOff();
            string text = "Batteries:\n";
            text += " Count: " + batteries.Count + "\n";
            text += " Total charge: " + Math.Round(chargePercent * 100, 1) + "%\n";
            text += " Time to " + (charging?"charge":"discharge") + ": " + (chargeTime.Hours>0?chargeTime.Hours+" Hr"+(chargeTime.Hours>1?"s ":" "):" ") + chargeTime.Minutes + " min\n";
            float batteriesDraw = 0f;
            float batteriesChargeDraw = 0f;
            foreach(IMyBatteryBlock b in batteries)
            {
                batteriesDraw += b.CurrentOutput;
                batteriesChargeDraw += b.CurrentInput;
            }
            text += " Power draw: " + Math.Round(batteriesDraw, 2) +"MW\n";
            text += " Re-charge power draw: " + Math.Round(batteriesChargeDraw, 2) + "MW\n";
            text += "\nReactors:\n";
            text += " Count: " + reactors.Count + "\n";
            float totalPower = 0f;
            foreach (IMyReactor r in reactors)
            {
                text += "  " + r.CustomName + " - Status: " + (r.IsEnabled() ? "On" : "Off") + "\n";
                totalPower += r.CurrentOutput;
            }

            text += " Power draw: " + Math.Round(totalPower, 2) + "MW\n";
            text += "\nSolar Panels:\n";
            text += " Count: " + panels.Count + "\n";
            foreach(IMySolarPanel p in panels)
            {
                maxPower += p.MaxOutput;
                curPower += p.CurrentOutput;
            }
            text += (" Max power draw: " + Math.Round(maxPower,2) + "MW\n" + 
                " Current power draw: " + Math.Round(curPower,2) + "MW\n");
            text += ("\nTotal power draw: " + Math.Round(curPower + totalPower + batteriesDraw, 2) + "MW\n");
            text += (" (Less re-charge power: " + Math.Round(curPower + totalPower + batteriesDraw - batteriesChargeDraw, 2) + "MW)\n");
            lcds.WriteToScreens(text);
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