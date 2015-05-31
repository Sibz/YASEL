using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Airvent
{
    class Airvent
    {
        public static void Depressurise(List<IMyTerminalBlock> vents)
        {
            vents.ForEach(vent => { Depressurise(vent as IMyAirVent); });
        }
        public static void Depressurise(IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_On").Apply(vent);
        }
        public static void Pressurise(List<IMyTerminalBlock> vents)
        {
            vents.ForEach(vent => { Pressurise(vent as IMyAirVent); });
        }
        public static void Pressurise(IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_Off").Apply(vent);
        }
    }
}