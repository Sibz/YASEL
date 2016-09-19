using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;

namespace TaskQueue
{

    using TimerExtensions;
    public class TaskQueue
    {
        IMyTimerBlock timer;
        Queue<Action> actions = new Queue<Action>();
        DateTime LastRunTime;
        int tickMS = 100;
        MyGridProgram gp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timer">Timer block to trigger loop, will 'start' timer when nothing to process, or trigger if there is stuff in the queue</param>
        /// <param name="tickMS">How many milliseconds to wait before processing queue again</param>
        public TaskQueue(MyGridProgram gp, IMyTimerBlock timer, int tickMS = 100)
        {
            if (timer == null)
                throw new Exception("TaskQueue Ctor: Timer can not be null");
            this.timer = timer;
            LastRunTime = DateTime.Now;
            this.tickMS = tickMS;
            this.gp = gp;
        }

        public void Enqueue(Action item)
        {
            actions.Enqueue(item);
        }

        public void Tick()
        {
            if (LastRunTime.AddMilliseconds(tickMS) > DateTime.Now)
                return;
            LastRunTime = DateTime.Now;
            while (gp.Runtime.CurrentInstructionCount < gp.Runtime.MaxInstructionCount / 2 &&
                gp.Runtime.CurrentMethodCallCount < gp.Runtime.MaxMethodCallCount / 2 &&
                actions.Count > 0)
                actions.Dequeue()();
            gp.Echo("Starting Timer - " + actions.Count);
            if (actions.Count > 0 && tickMS < timer.TriggerDelay)
                timer.Trigger();
            else
                timer.Start();
        }
    }
}
