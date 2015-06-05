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
    using Grid;
    using Door;
    using TextPanel;
    using Block;

    /// <summary>
    /// General station management functions
    /// </summary>
    public class StationManager
    {
        StationManagerSettings m_settings;
        Dictionary<IMyDoor, DateTime> m_doorsToClose;

        public StationManager(StationManagerSettings settings)
        {
            m_settings = settings;
            m_doorsToClose = new Dictionary<IMyDoor, DateTime>();
        }

        /// <summary>
        /// Will automatically close doors after <i>AutoDoorCloseTime</i>
        /// By default will only close doors with 'auto' in the name, however can be changed by modifying <i>AutoDoorCloseWord</i>
        /// </summary>
        public void ManageAutoDoors()
        {
            var doors = new List<IMyTerminalBlock>();
            Grid.ts.GetBlocksOfType<IMyDoor>(doors, delegate(IMyTerminalBlock b) { return b.CustomName.Contains(m_settings.AutoDoorCloseWord) && Grid.BelongsToGrid(b); });
            doors.ForEach(door =>
            {
                if ((door as IMyDoor).OpenRatio >= 1 && !m_doorsToClose.ContainsKey((door as IMyDoor)))
                    m_doorsToClose.Add((door as IMyDoor), DateTime.Now);
            });
            var doorEnum = m_doorsToClose.GetEnumerator();
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
                m_doorsToClose.Remove(door);
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

        /// <summary>
        /// Manages the oxygen level in tanks
        /// </summary>
        public void ManageOxygen()
        {
            var tanks = new List<IMyTerminalBlock>();
            var gens = new List<IMyTerminalBlock>();
            var farms = new List<IMyTerminalBlock>();
            if (m_settings.OxygenTankGroup == "")
                Grid.ts.GetBlocksOfType<IMyOxygenTank>(tanks, Grid.BelongsToGrid);
            else
                tanks = Grid.GetBlockGrp(m_settings.OxygenTankGroup);
            if (m_settings.OxygenGenGroup == "")
                Grid.ts.GetBlocksOfType<IMyOxygenGenerator>(gens, Grid.BelongsToGrid);
            else
                gens = Grid.GetBlockGrp(m_settings.OxygenGenGroup);
            if (m_settings.OxygenFarmGroup == "")
                Grid.ts.GetBlocksOfType<IMyOxygenFarm>(farms, Grid.BelongsToGrid);
            else
                farms = Grid.GetBlockGrp(m_settings.OxygenFarmGroup);
            if (tanks.Count == 0)
                return;
            if (gens.Count == 0)
                return;
            if (tanks.IsValidIndex(0) && (tanks[0] as IMyOxygenTank) != null && (tanks[0] as IMyOxygenTank).GetOxygenLevel() * 100 > m_settings.OxygenMaxLevel)
            {
                Block.TurnOnOff(gens, false);
                if (m_settings.ManageOxygenFarm) Block.TurnOnOff(farms, false);
            }
            else if (tanks.IsValidIndex(0) && (tanks[0] as IMyOxygenTank) != null && (tanks[0] as IMyOxygenTank).GetOxygenLevel() * 100 < m_settings.OxygenMinLevel)
            {
                Block.TurnOnOff(gens);
                if (m_settings.ManageOxygenFarm) Block.TurnOnOff(farms);
            }
        }



    }

    /// <summary>
    /// Settings to initialise StationManager with
    /// </summary>
    public class StationManagerSettings
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
        public string TextPanelTimeName = "";

        /// <summary>
        /// Optional group name of tanks to manage<br />
        /// if not specified, all tanks are managed
        /// </summary>
        public string OxygenTankGroup = "";

        /// <summary>
        /// Optional group name of generators to manage
        /// if not specified all generators are managed
        /// </summary>
        public string OxygenGenGroup = "";

        /// <summary>
        /// Optional group name of farms to manage
        /// if not specified all farms are managed
        /// </summary>
        public string OxygenFarmGroup = "";

        /// <summary>
        /// Max level of oxygen in tank / the level at which gens turn off
        /// </summary>
        public float OxygenMaxLevel = 90f;

        /// <summary>
        /// Min level of oxygen in tanks / the level at which gens turn on
        /// </summary>
        public float OxygenMinLevel = 10f;

        /// <summary>
        /// Turn farms on/off along with generators.
        /// </summary>
        public bool ManageOxygenFarm = true;

    }
}