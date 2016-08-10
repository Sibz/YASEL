using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace Disassembler
{
    using InventoryExtensions;
    using ProgramExtensions;
    using TextPanelExtensions;
    
    public class Disassembler
    {
        MyGridProgram gp;
        DisassemblerSettings s;

        public Disassembler(MyGridProgram gp,DisassemblerSettings settings)
        {
            this.gp = gp;
            s = settings;
        }

        public void FillDisassemblers()
        {
            var cargos = gp.GetBlockGroup(s.ComponentsCargoGroup);
            var assemblers = gp.GetBlockGroup(s.DisassemblerGroup);
            var invCargos = cargos.GetInventories();
            var invAssemblers = assemblers.GetInventories(1);
            gp.Echo("Block Counts:" + cargos.Count + " / " + assemblers.Count);
            gp.Echo("Inv Counts:" + invCargos.Count + " / " + invAssemblers.Count);
            var targetValues = (gp.GetBlock(s.LCDTargetValues) as IMyTextPanel).GetValueList();

            var tvEnum = targetValues.GetEnumerator();
            while (tvEnum.MoveNext())
            {
                var target = (tvEnum.Current.Value * (1 + s.OveragePercent));
                var count = Math.Round(invCargos.CountItems(tvEnum.Current.Key) - target);
                gp.Echo("Checking " + tvEnum.Current.Key);
                gp.Echo("Count:" + invCargos.CountItems(tvEnum.Current.Key) + " target:" + target + " = " + count);
                if (count > 0)
                    invCargos.MoveItemAmount(invAssemblers, tvEnum.Current.Key, (VRage.MyFixedPoint)count);
            }
            
        }
    }

    public class DisassemblerSettings
    {
        public string ComponentsCargoGroup;
        public string DisassemblerGroup;
        public string LCDTargetValues = "LCDList";
        public double OveragePercent = 0.25;
    }
}