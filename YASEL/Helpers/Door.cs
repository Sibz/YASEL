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
    }
}