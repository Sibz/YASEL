using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage;
using VRageMath;


namespace StationManager
{
    using Str;
    using Grid;
    using Door;

    /// <summary>
    /// General station management functions
    /// </summary>
    class StationManager
    {
        StationManagerSettings m_settings;
        Dictionary<IMyDoor, DateTime> DoorsToClose;

        public StationManager(StationManagerSettings settings)
        {
            m_settings = settings;
            DoorsToClose = new Dictionary<IMyDoor, DateTime>();
        }

        /// <summary>
        /// Will automatically close doors after AutoDoorCloseTime
        /// </summary>
        public void ManageAutoDoors()
        {
            var doors = new List<IMyTerminalBlock>();
            Grid.ts.GetBlocksOfType<IMyDoor>(doors, delegate(IMyTerminalBlock b) { return Str.Contains(b.CustomName, "auto") && Grid.BelongsToGrid(b); });
            doors.ForEach(door =>
            {
                if ((door as IMyDoor).OpenRatio>=1 && !DoorsToClose.ContainsKey((door as IMyDoor)))
                    DoorsToClose.Add((door as IMyDoor), DateTime.Now);
            });
            var doorEnum = DoorsToClose.GetEnumerator();
            var closedDoors = new List<IMyDoor>();
            while (doorEnum.MoveNext())
            {
                if (DateTime.Now.AddSeconds(-m_settings.AutoDoorCloseTime) >= doorEnum.Current.Value)
                {
                    Door.Close(doorEnum.Current.Key);
                    closedDoors.Add(doorEnum.Current.Key);
                }
            }
            closedDoors.ForEach(door =>
            {
                DoorsToClose.Remove(door);
            });
        }
    }

    /// <summary>
    /// Settings to initialise StationManager with
    /// </summary>
    class StationManagerSettings
    {
        /// <summary>
        /// Number of seconds before closing an open door
        /// </summary>
        public int AutoDoorCloseTime = 5;
    }
}