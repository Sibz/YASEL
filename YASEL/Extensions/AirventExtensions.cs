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
        static public List<IMyAirVent> ToAirVents(this List<IMyTerminalBlock> bs)
        {
            var av = new List<IMyAirVent>();
            bs.ForEach(b => { if (b is IMyAirVent) av.Add(b as IMyAirVent); });
            return av;
        }

        static public void Depressurise(this List<IMyAirVent> vents)
        {
            vents.ForEach(vent => { vent.Depressurise(); });
        }

        static public void Depressurise(this IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_On").Apply(vent);
        }

        static public void Pressurise(this List<IMyAirVent> vents)
        {
            vents.ForEach(vent => { vent.Pressurise(); });
        }

        static public void Pressurise(this IMyAirVent vent)
        {
            vent.GetActionWithName("Depressurize_Off").Apply(vent);
        }
    }
}