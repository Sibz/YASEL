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
    using TextPanel;

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
        /// Will automatically close doors after <i>AutoDoorCloseTime</i>
        /// By default will only close doors with 'auto' in the name, however can be changed by modifying <i>AutoDoorCloseWord</i>
        /// </summary>
        public void ManageAutoDoors()
        {
            var doors = new List<IMyTerminalBlock>();
            Grid.ts.GetBlocksOfType<IMyDoor>(doors, delegate(IMyTerminalBlock b) { return Str.Contains(b.CustomName, m_settings.AutoDoorCloseWord) && Grid.BelongsToGrid(b); });
            doors.ForEach(door =>
            {
                if ((door as IMyDoor).OpenRatio >= 1 && !DoorsToClose.ContainsKey((door as IMyDoor)))
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

        /// <summary>
        /// Display time on text panel defined by <i>TextPanelTimeName</i>.
        /// </summary>
        /// <param name="prePadding"></param>
        /// <param name="postPadding"></param>
        /// <param name="format"></param>
        public void DisplayTime(string prePadding = "", string postPadding = "", string format = "HH:mm")
        {
            if (m_settings.TextPanelTimeName == "")
                throw new Exception("DisplayTime: TextPanelTimeName is empty");

            var myTextPanel = Grid.GetBlock(m_settings.TextPanelTimeName) as IMyTextPanel;
            if (myTextPanel == null)
                throw new Exception("DisplayTime: Unable to access TextPanel: " + m_settings.TextPanelTimeName);
            TextPanel.Write(myTextPanel, prePadding + DateTime.Now.ToString(format) + postPadding, false);
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

        /// <summary>
        /// Doors with this in there name will be closed automatically
        /// </summary>
        public string AutoDoorCloseWord = "auto";

        /// <summary>
        /// Textpanel to display time on when calling DisplayTime
        /// </summary>
        public string TextPanelTimeName;
    }
}