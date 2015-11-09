using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManager
{
    using GridHelper;
    using Block;
    using Door;
    using Airvent;

    public class AirlockManager
    {
        
        AirlockManagerSettings m_settings;
        Dictionary<string, Airlock> m_airlocks;
        GridHelper gh;

        public AirlockManager(GridHelper gh, AirlockManagerSettings settings)
        {
            this.gh = gh;
            m_settings = settings;
            m_airlocks = new Dictionary<string, Airlock>();
            var airlockSensors = new List<IMyTerminalBlock>();
            gh.Gts.GetBlocksOfType<IMySensorBlock>(airlockSensors, delegate(IMyTerminalBlock b)
            {
                return (b.CustomName.Contains("airlock") && gh.BelongsToGrid(b));
            });
            airlockSensors.ForEach(sensor =>
            {
                var names = sensor.CustomName.Split(' ');
                if (names.Length >= 2 && !m_airlocks.ContainsKey(names[2]))
                {
                    m_airlocks.Add(names[2], new Airlock(gh, names[2]));
                    if (m_settings.OnAirlockUpdate != null) m_airlocks[names[2]].OnUpdate = m_settings.OnAirlockUpdate;
                }
            });
        }
        /// <summary>
        /// Airlock Automation<br />
        /// Name components as follows<br />
        /// Airlock Sensor AIRLOCKNAME x - Sensor(s) covering Exit<br />
        /// xx AIRLOCKNAME x - Airvent(s) for airlock<br />
        /// xx AIRLOCKNAME Ex x - Exterior doors<br />
        /// xx AIRLOCKNAME In x - Interior doors<br />
        /// *x can be a number (or word(s)) or ommited<br />
        /// *xx can be word or words or ommited<br />
        /// Example: An airlock called MyAirlock<br />
        /// Sensor Name: Airlock Sensor MyAirlock<br />
        /// Airvent Name: Airvent MyAirlock<br />
        /// External Door: Hangar Door MyAirlock Ex 1, Hangar Door MyAirlock Ex 2, etc<br />
        /// Internal Door: Door MyAirlock In<br />
        /// </summary>
        public void ManageAirlocks()
        {
            var AirlockEnum = m_airlocks.GetEnumerator();
            while (AirlockEnum.MoveNext())
            {
                AirlockEnum.Current.Value.Tick();
            }
        }
        public class Airlock
        {
            string m_name, m_state;
            List<IMyTerminalBlock> m_sensors;
            List<IMyTerminalBlock> m_airvents;
            List<IMyTerminalBlock> m_doorsEx;
            List<IMyTerminalBlock> m_doorsIn;
            DateTime m_lastPressureChangeTime;
            float m_lastPressureChangeValue;
            public Action<string, string, float> OnUpdate;
            GridHelper gh;

            public Airlock(GridHelper gh, string airlockName)
            {
                this.gh = gh;
                m_name = airlockName;
                m_state = "init";
                m_sensors = new List<IMyTerminalBlock>();
                m_airvents = new List<IMyTerminalBlock>();
                m_doorsEx = new List<IMyTerminalBlock>();
                m_doorsIn = new List<IMyTerminalBlock>();
                gh.Gts.GetBlocksOfType<IMySensorBlock>(m_sensors,
                    delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(airlockName) && gh.BelongsToGrid(b)); });
                gh.Gts.GetBlocksOfType<IMyAirVent>(m_airvents,
                    delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(airlockName) && gh.BelongsToGrid(b)); });
                gh.Gts.GetBlocksOfType<IMyDoor>(m_doorsEx,
                    delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(airlockName + " Ex") && gh.BelongsToGrid(b)); });
                gh.Gts.GetBlocksOfType<IMyDoor>(m_doorsIn,
                    delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(airlockName + " In") && gh.BelongsToGrid(b)); });
            }
            public void Tick()
            {
                if (m_state == "init")
                {
                    initialise();
                }
                else if ((m_state == "idle" || m_state == "opening" || m_state == "pressurise") && sensorActive())
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
                else if (m_state == "pressurise" && !sensorActive())
                {
                    deactivate();
                }
                else if ((m_state == "open" || m_state == "closing" || m_state == "depressurise") && !sensorActive())
                {
                    close();
                }
                else if (m_state == "closing" && sensorActive())
                {
                    cancelClose();
                }
                if (OnUpdate != null) OnUpdate(m_name, m_state, (m_airvents.IsValidIndex(0) ? (m_airvents[0] as IMyAirVent).GetOxygenLevel() : 0));
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
                if (m_airvents.IsValidIndex(0) && m_lastPressureChangeValue != (m_airvents[0] as IMyAirVent).GetOxygenLevel())
                {
                    m_lastPressureChangeTime = DateTime.Now;
                    m_lastPressureChangeValue = (m_airvents[0] as IMyAirVent).GetOxygenLevel();
                }
                if ((m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() == 0) ||
                    (DateTime.Now - m_lastPressureChangeTime).TotalSeconds >= 3)
                {
                    Block.TurnOnOff(m_doorsEx);
                    Door.Open(m_doorsEx);
                    if (Door.IsOpen(m_doorsEx))
                    {
                        m_lastPressureChangeValue = -1;
                        Block.TurnOnOff(m_doorsEx, false);
                        m_state = "open";
                    }
                }
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
                Airvent.Pressurise(m_airvents);
                if (m_airvents.IsValidIndex(0) && m_lastPressureChangeValue != (m_airvents[0] as IMyAirVent).GetOxygenLevel())
                {
                    m_lastPressureChangeTime = DateTime.Now;
                    m_lastPressureChangeValue = (m_airvents[0] as IMyAirVent).GetOxygenLevel();
                }
                if (m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() > 0.75 ||
                    (DateTime.Now - m_lastPressureChangeTime).TotalSeconds >= 3)
                {
                    m_lastPressureChangeValue = -1;
                    m_state = "idle";
                }
            }
            void initialise()
            {
                Block.TurnOnOff(m_doorsEx);
                Block.TurnOnOff(m_doorsIn);
                Door.Close(m_doorsEx);
                if (!Door.IsClosed(m_doorsEx))
                    return;
                Airvent.Pressurise(m_airvents);
                if (m_airvents.IsValidIndex(0) && m_lastPressureChangeValue != (m_airvents[0] as IMyAirVent).GetOxygenLevel())
                {
                    m_lastPressureChangeTime = DateTime.Now;
                    m_lastPressureChangeValue = (m_airvents[0] as IMyAirVent).GetOxygenLevel();
                }
                if (!(m_airvents.IsValidIndex(0) && (m_airvents[0] as IMyAirVent).GetOxygenLevel() > 0.75 ||
                    (DateTime.Now - m_lastPressureChangeTime).TotalSeconds >= 3))
                    return;
                m_lastPressureChangeValue = -1;
                Door.Open(m_doorsIn);
                if (!Door.IsOpen(m_doorsIn))
                    return;
                Block.TurnOnOff(m_doorsEx, false);
                Block.TurnOnOff(m_doorsIn, false);
                m_state = "idle";
            }

        }
    }

    public class AirlockManagerSettings
    {
        public Action<string, string, float> OnAirlockUpdate;
    }
    
}