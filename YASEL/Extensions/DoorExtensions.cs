using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace DoorExtensions
{
    using BlockExtensions;

    static class DoorExtensions
    {
        public static List<IMyDoor> ToDoors(this List<IMyTerminalBlock> bs)
        {
            var l = new List<IMyDoor>();
            bs.ForEach(b => { if (b is IMyDoor) l.Add(b as IMyDoor); });
            return l;
        }

        static public void DoOpen(this IMyDoor door)
        {
            door.GetActionWithName("Open_On").Apply(door);
        }
        static public void DoOpen(this List<IMyDoor> doors)
        {
            doors.ForEach(door => { door.DoOpen(); });
        }
        static public void DoClose(this IMyDoor door)
        {
            door.GetActionWithName("Open_Off").Apply(door);
        }
        static public void DoClose(this List<IMyDoor> doors)
        {
            doors.ForEach(door => { door.DoClose(); });
        }
        static public bool IsClosed(this List<IMyDoor> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!d.IsClosed()) rVal = false; });
            return rVal;
        }
        static public bool IsClosed(this IMyDoor door)
        {
            if (door.OpenRatio != 0f) return false;
            return true;
        }
        static public bool IsOpen(this List<IMyDoor> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!d.IsOpen()) rVal = false; });
            return rVal;
        }
        static public bool IsOpen(this IMyDoor door)
        {
            if (door.OpenRatio == 1f) return true;
            return false;
        }
        
        static public bool CloseAndLockDoor(this List<IMyDoor> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!d.CloseAndLockDoor()) rVal = false; });
            return rVal;
        }
        static public bool CloseAndLockDoor(this IMyDoor door)
        {
            if (!door.IsClosed())
            {
                door.TurnOn();
                door.DoClose();
            }
            else if (door.IsClosed())
            {
                door.TurnOff();
                return true;
            }
            return false;
        }
        static public bool OpenAndLockDoor(this List<IMyDoor> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!d.OpenAndLockDoor()) rVal = false; });
            return rVal;
        }
        static public bool OpenAndLockDoor(this IMyDoor door)
        {
            if (!door.IsOpen())
            {
                door.TurnOn();
                door.DoOpen();
            }
            else if (door.IsOpen())
            {
                door.TurnOff();
                return true;
            }
            return false;
        }
    }
}