using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerExtensions
{
    public static class TimerExtensions
    {
        public static void Trigger(this IMyTimerBlock timer)
        {
            timer.GetActionWithName("TriggerNow").Apply(timer);
        }
        public static void Start(this IMyTimerBlock timer)
        {
            timer.GetActionWithName("Start").Apply(timer);
        }
    }
   
}
