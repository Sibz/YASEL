using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace BatteryManager
{
    using BatteryExtensions;
    
    class BatteryManager
    {
        public List<IMyTerminalBlock> Batteries;

        public BatteryManager()
        {
            Batteries = new List<IMyTerminalBlock>();
        }
        public BatteryManager(MyGridProgram gp, bool ongrid = true)
        {
            Batteries = new List<IMyTerminalBlock>();
            if (ongrid)
                gp.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Batteries, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Batteries);
        }

        public BatteryManager(List<IMyTerminalBlock> bats)
        {
            this.Batteries = bats;
        }
        public void Recharge(bool on = true)
        {
            Batteries.ForEach(b => { if (b is IMyBatteryBlock) (b as IMyBatteryBlock).Recharge(on); });
        }
        public void Discharge(bool on = true)
        {
            Batteries.ForEach(b => { if (b is IMyBatteryBlock) (b as IMyBatteryBlock).Discharge(on); });
        }
    }
}