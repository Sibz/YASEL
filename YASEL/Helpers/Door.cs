using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace Door
{
    using Str;
    static class Door
    {
        static public void Open(IMyDoor door)
        {
            if (door == null)
                throw new Exception("Door.Open: Null Argument");
            door.GetActionWithName("Open_On").Apply(door);
        }
        static public void Close(IMyDoor door)
        {
            if (door == null)
                throw new Exception("Door.Close: Null Argument");
            door.GetActionWithName("Open_Off").Apply(door);
        }
        static public bool IsClosed(List<IMyTerminalBlock> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!IsClosed(d as IMyDoor)) rVal = false; });
            return rVal;
        }
        static public bool IsClosed(IMyDoor door)
        {
            if (door == null)
                throw new Exception("Door.IsClosed: Null Argument");
            if (door.OpenRatio != 0) return false;
            return true;
        }
        static public bool IsOpen(List<IMyTerminalBlock> doors)
        {
            bool rVal = true;
            doors.ForEach(d => { if (!IsOpen(d as IMyDoor)) rVal = false; });
            return rVal;
        }
        static public bool IsOpen(IMyDoor door)
        {
            if (door == null)
                throw new Exception("Door.IsOpen: Null Argument");
            if (door.OpenRatio == (Str.Contains(door.BlockDefinition.ToString(),"hangar") ? 1 : 1.2)) return false;
            return true;
        }
    }
}