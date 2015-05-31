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
    using Block;
    using Airvent;

    /// <summary>
    /// General station management functions
    /// </summary>
    class StationManager
    {
        StationManagerSettings m_settings;
        Dictionary<IMyDoor, DateTime> m_doorsToClose;
        Dictionary<string, Airlock> m_airlocks;

        public StationManager(StationManagerSettings settings)
        {
            m_settings = settings;
            m_doorsToClose = new Dictionary<IMyDoor, DateTime>();
            initAirlocks();
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
        /// Airlock Automation<br />
        /// Name components as follows<br />
        /// Airlock Sensor AIRLOCKNAME x - Sensor(s) covering Exit<br />
        /// Airvent AIRLOCKNAME x - Airvent(s) for airlock<br />
        /// Door AIRLOCKNAME Ex x - Exterior doors<br />
        /// Door AIRLOCKNAME In x - Interior doors<br />
        /// Light AIRLOCKNAME Ex x - Interior lights<br />
        /// Light AIRLOCKNAME In x - Interior lights<br />
        /// LCD AIRLOCKNAME x - TextPanel/LCD for status<br />
        /// *x can be a number or ommited
        /// </summary>
        public void ManageAirlocks()
        {
            var AirlockEnum = m_airlocks.GetEnumerator();
            while (AirlockEnum.MoveNext())
            {
                AirlockEnum.Current.Value.Tick();
            }
        }
        class Airlock
        {
            string m_name, m_state;
            List<IMyTerminalBlock> m_sensors;
            List<IMyTerminalBlock> m_airvents;
            List<IMyTerminalBlock> m_doorsEx;
            List<IMyTerminalBlock> m_doorsIn;
            public Action<string, float> OnUpdate;

            public Airlock(string airlockName)
            {
                m_name = airlockName;
                m_state = "idle";
                Grid.ts.GetBlocksOfType<IMySensorBlock>(m_sensors,
                    delegate(IMyTerminalBlock b) { return (Str.Contains(b.CustomName, airlockName) && Grid.BelongsToGrid(b)); });
                Grid.ts.GetBlocksOfType<IMyAirVent>(m_airvents,
                    delegate(IMyTerminalBlock b) { return (Str.Contains(b.CustomName, "Airvent " + airlockName) && Grid.BelongsToGrid(b)); });
                Grid.ts.GetBlocksOfType<IMyDoor>(m_doorsEx,
                    delegate(IMyTerminalBlock b) { return (Str.Contains(b.CustomName, "Door " + airlockName + " Ex") && Grid.BelongsToGrid(b)); });
                Grid.ts.GetBlocksOfType<IMyDoor>(m_doorsIn,
                    delegate(IMyTerminalBlock b) { return (Str.Contains(b.CustomName, "Door " + airlockName + " In") && Grid.BelongsToGrid(b)); });
            }
            public void Tick()
            {
                if (m_state == "idle" && sensorActive())
                {
                    activate();
                }
                else if ((m_state == "opening" || m_state == "idle") && !sensorActive())
                {
                    cancelActivate();
                }
                else if (m_state == "depressurise" && sensorActive())
                {
                    open();
                }
                else if (m_state == "depressurise" && !sensorActive())
                {
                    cancelOpen();
                } 
                else if (m_state == "open" && !sensorActive())
                {
                    close();
                }
                else if (m_state == "closing" && sensorActive())
                {
                    cancelClose();
                }
                else if (m_state == "pressurise" && !sensorActive())
                {
                    deactivate();
                }
                else if (m_state == "pressurise" && sensorActive())
                {
                    activate();
                }
                if (OnUpdate != null) OnUpdate(m_state, (m_airvents.IsValidIndex(0) ? (m_airvents as IMyAirVent).GetOxygenLevel() : 0));
            }
            bool sensorActive()
            {
                bool rVal = false;
                m_sensors.ForEach(sensor =>
                {
                    if ((sensor as IMySensorBlock).IsActive) rVal = true;
                });
                return rVal;
            }
            void activate()
            {
                m_state = "opening";
                Block.TurnOnOff(m_doorsIn);
                Door.Close(m_doorsIn);
                if (Door.IsClosed(m_doorsIn))
                {
                    Block.TurnOnOff(m_doorsIn, false);
                    Airvent.Depressurise(m_airvents);
                    m_state = "depressurise";
                }
            }
            void cancelActivate()
            {
                Block.TurnOnOff(m_doorsIn);
                Door.Open(m_doorsIn);
                if (Door.IsOpen(m_doorsIn))
                {
                    Block.TurnOnOff(m_doorsIn, false);
                    Block.TurnOnOff(m_doorsEx, false);
                    m_state = "idle";
                }
            }
            void open()
            {
                if (m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() == 0)
                {
                    Block.TurnOnOff(m_doorsEx);
                    Door.Open(m_doorsEx);
                    if (Door.IsOpen(m_doorsEx))
                    {
                        Block.TurnOnOff(m_doorsEx, false);
                        m_state = "open";
                    }
                }
            }
            void cancelOpen()
            {
                Airvent.Pressurise(m_airvents);
                if (m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() > 0.75)
                    m_state = "idle";
            }
            void close()
            {
                m_state = "closing";
                Block.TurnOnOff(m_doorsEx);
                Door.Close(m_doorsEx);
                if (Door.IsClosed(m_doorsEx))
                {
                    Block.TurnOnOff(m_doorsEx, false);
                    Airvent.Pressurise(m_airvents);
                    m_state = "pressurise";
                }
            }
            void cancelClose()
            {
                Door.Open(m_doorsEx);
                if (Door.IsOpen(m_doorsEx))
                {
                    Block.TurnOnOff(m_doorsEx, false);
                    m_state = "open";
                }
            }
            void deactivate()
            {
                if (m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() > 0.75)
                    m_state = "idle";
            }

        }
        void initAirlocks()
        {
            var AirlockSensors = new List<IMyTerminalBlock>();
            Grid.ts.GetBlocksOfType<IMySensorBlock>(AirlockSensors, delegate(IMyTerminalBlock b)
            {
                return (Str.Contains(b.CustomName, "airlock") && Grid.BelongsToGrid(b));
            });
            AirlockSensors.ForEach(sensor =>
            {
                var names = sensor.CustomName.Split(' ');
                if (names.IsValidIndex(2) && !m_airlocks.ContainsKey(names[2]))
                {
                    m_airlocks.Add(names[2], new Airlock(names[2]));
                }
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