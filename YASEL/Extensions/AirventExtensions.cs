using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirventExtensions
{
    static class AirventExtensions
    {
       
        static public void Depressurise(this IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_On").Apply(vent);
        }

        static public void Pressurise(this IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_Off").Apply(vent);
        }
    }
}