using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace DoorManager
{
    using DoorExtensions;
    class DoorManager
    {
        public List<IMyTerminalBlock> Doors;
        public DoorManager()
        {
            Doors = new List<IMyTerminalBlock>();
        }
        public DoorManager(MyGridProgram gp, bool ongrid = true)
        {
            Doors = new List<IMyTerminalBlock>();
            if (ongrid)
                gp.GridTerminalSystem.GetBlocksOfType<IMyDoor>(Doors, b => { return b.CubeGrid == gp.Me.CubeGrid; });
            else
                gp.GridTerminalSystem.GetBlocksOfType<IMyDoor>(Doors);
        }

        public DoorManager(List<IMyTerminalBlock> doors)
        {
            Doors = doors;
        }
        public void Open()
        {
            Doors.ForEach(door => { if (door is IMyDoor) (door as IMyDoor).Open(); });
        }
        public void Shut()
        {
            Doors.ForEach(door => { if (door is IMyDoor) (door as IMyDoor).Shut(); });
        }
        public bool CloseAndLock()
        {
            bool complete = true;
            Doors.ForEach(door => { if (door is IMyDoor) complete &= (door as IMyDoor).CloseAndLock(); });
            return complete;
        }
        public bool OpenAndLock()
        {
            bool complete = true;
            Doors.ForEach(door => { if (door is IMyDoor) complete &= (door as IMyDoor).OpenAndLock(); });
            return complete;
        }
        public bool AreAllClosed()
        {
            bool closed = true;
            Doors.ForEach(door => { if (door is IMyDoor) closed &= (door as IMyDoor).IsClosed(); });
            return closed;
        }
        public bool AreAllOpen()
        {
            bool open = true;
            Doors.ForEach(door => { if (door is IMyDoor) open &= (door as IMyDoor).IsOpen(); });
            return open;
        }

    }
}