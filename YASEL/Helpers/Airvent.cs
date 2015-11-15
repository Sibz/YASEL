using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Airvent
{
    static class Airvent
    {
        static public void Depressurise(List<IMyTerminalBlock> vents)
        {
            vents.ForEach(vent => { if (vent is IMyAirVent) Depressurise(vent as IMyAirVent); });
        }
        static public void Depressurise(IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_On").Apply(vent);
        }
        static public void Pressurise(List<IMyTerminalBlock> vents)
        {
            vents.ForEach(vent => { if (vent is IMyAirVent) Pressurise(vent as IMyAirVent); });
        }
        static public void Pressurise(IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_Off").Apply(vent);
        }
    }
}