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
       
        static public void Open(this IMyDoor door)
        {
            door.GetActionWithName("Open_On").Apply(door);
        }
        
        static public void Shut(this IMyDoor door)
        {
            door.GetActionWithName("Open_Off").Apply(door);
        }
        
        
        static public bool IsClosed(this IMyDoor door)
        {
            if (door.OpenRatio != 0f) return false;
            return true;
        }
       
        static public bool IsOpen(this IMyDoor door)
        {
            if (door.OpenRatio == 1f) return true;
            return false;
        }
        
        
        static public bool CloseAndLockDoor(this IMyDoor door)
        {
            if (!door.IsClosed())
            {
                door.TurnOn();
                door.Shut();
            }
            else if (door.IsClosed())
            {
                door.TurnOff();
                return true;
            }
            return false;
        }
       
        static public bool OpenAndLockDoor(this IMyDoor door)
        {
            if (!door.IsOpen())
            {
                door.TurnOn();
                door.Open();
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