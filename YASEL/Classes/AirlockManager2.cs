using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManager2
{
    using GridHelper;
    using Airvent;
    using Door;
    using Block;

    class AirlockManager
    {
        GridHelper gh;

        Dictionary<string, AirlockNP> m_airlocks;

        public Action<string, string, float> OnUpdate;

        public AirlockManager(GridHelper gh, Action<string, string, float> onUpdate = null)
        {
            this.gh = gh;
            m_airlocks = new Dictionary<string, AirlockNP>();
            if (onUpdate != null)
                OnUpdate = onUpdate;
        }

        public void Tick()
        {
            updateAirlocksTick();
        }

        public void AddAirlockNP(string airlockName, string outerDoorName, string innerDoorName)
        {
            var newAirlock = new AirlockNP(gh, airlockName, outerDoorName, innerDoorName, OnUpdate);
            if (!m_airlocks.ContainsKey(airlockName))
                m_airlocks.Add(airlockName, newAirlock);
            else
                m_airlocks[airlockName] = newAirlock;
        }

        public void AddAirlock(string airlockName, string outerDoorName, string airventName, string innerDoorName = "")
        {
            var newAirlock = new Airlock(gh, airlockName, outerDoorName, innerDoorName, airventName, OnUpdate );
            if (!m_airlocks.ContainsKey(airlockName))
                m_airlocks.Add(airlockName, newAirlock);
            else
                m_airlocks[airlockName] = newAirlock;
        }

        public void OpenAirlock(string airlockName)
        {
            if (m_airlocks.ContainsKey(airlockName))
                m_airlocks[airlockName].Open();
            else
                throw new Exception("AirlockManager: Unable to open airlock, name not found");
        }

        public void CloseAirlock(string airlockName)
        {
            if (m_airlocks.ContainsKey(airlockName))
                m_airlocks[airlockName].Close();
            else
                throw new Exception("AirlockManager: Unable to close airlock, name not found");
        }

        private void updateAirlocksTick()
        {
            var alEnum = m_airlocks.GetEnumerator();
            while (alEnum.MoveNext())
                    alEnum.Current.Value.Tick();
        }
    }

    class AirlockNP
    {
        GridHelper gh;

        protected string m_airlockName, m_outerDoorName, m_innerDoorName;
        
        protected List<IMyTerminalBlock> m_innerDoors;
        protected List<IMyTerminalBlock> m_outerDoors;
        
        protected Action<string, string, float> m_onUpdate;

        protected const string STATE_SEALING = "Sealing";
        protected const string STATE_OPENING = "Opening";
        protected const string STATE_CLOSING = "Closing";
        protected const string STATE_UNSEALING = "Unsealing";
        protected const string STATE_IDLE = "Idle";

        protected string m_state = STATE_IDLE;

        public AirlockNP(GridHelper gh, string airlockName, string outerDoorName, string innerDoorName, Action<string, string, float> onUpdate)
        {
            m_airlockName = airlockName;
            m_outerDoorName = outerDoorName;
            m_innerDoorName = innerDoorName;
            m_onUpdate = onUpdate;
            m_innerDoors = new List<IMyTerminalBlock>();
            m_outerDoors = new List<IMyTerminalBlock>();

            if (m_innerDoorName != "")
                gh.Gts.GetBlocksOfType<IMyDoor>(m_innerDoors, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(m_innerDoorName) && gh.BelongsToGrid(b)); });
            gh.Gts.GetBlocksOfType<IMyDoor>(m_outerDoors, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(outerDoorName) && gh.BelongsToGrid(b)); });

            if (m_outerDoors.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + m_airlockName + "' -  Outer doors not found");
        }

        public void Open()
        {
            m_state = STATE_SEALING;
        }
        public void Close()
        {
            m_state = STATE_CLOSING;
        }
        public virtual void Tick()
        {
            if (m_innerDoors.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + m_airlockName + "' -  Inner doors not found");
            if (m_state == STATE_SEALING)
            {
                if (Door.CloseAndLockDoor(m_innerDoors))
                    m_state = STATE_OPENING;
            }
            else if (m_state == STATE_OPENING)
            {

                if (Door.IsOpen(m_innerDoors))
                {
                    m_state = STATE_SEALING;
                    return;
                }
                if (Door.OpenAndLockDoor(m_outerDoors))
                    m_state = STATE_IDLE;
            }
            else if (m_state == STATE_CLOSING)
            {
                if (Door.CloseAndLockDoor(m_outerDoors))
                    m_state = STATE_UNSEALING;
            }
            else if (m_state == STATE_UNSEALING)
            {
                if (Door.IsOpen(m_outerDoors))
                {
                    m_state = STATE_CLOSING;
                    return;
                }
                if (Door.OpenAndLockDoor(m_innerDoors))
                    m_state = STATE_IDLE;
            }
            if (m_onUpdate != null)
                m_onUpdate(m_airlockName, m_state, 0);
        }
    }

    class Airlock : AirlockNP
    {
        string m_airventName;

        const string STATE_DEPRESSURISING = "Depressurising";
        const string STATE_PRESSURISING = "Pressurising";
       

        List<IMyTerminalBlock> m_airvents;
        List<IMyTerminalBlock> m_oxyTanks;
        

        public Airlock(GridHelper gh, string airlockName, string outerDoorName, string innerDoorName, string airventName, Action<string, string, float> onUpdate) : 
            base(gh,airlockName,outerDoorName,innerDoorName,onUpdate)
        {
            m_airventName = airventName;

            m_airvents = new List<IMyTerminalBlock>();
            m_oxyTanks = new List<IMyTerminalBlock>();

            m_onUpdate = onUpdate;

            gh.Gts.GetBlocksOfType<IMyOxygenTank>(m_oxyTanks, gh.BelongsToGrid);

            gh.Gts.GetBlocksOfType<IMyAirVent>(m_airvents, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(m_airventName) && gh.BelongsToGrid(b)); });
            if (m_airvents.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + m_airlockName + "' -  Vents not found");
        }


        public override void Tick()
        {
            if (m_state == STATE_SEALING)
            {
                // If we have inner doors, close them
                if (m_innerDoors.Count>0)
                    Door.Close(m_innerDoors);
                m_state = STATE_DEPRESSURISING;
            }
            else if (m_state == STATE_DEPRESSURISING)
            {
                // Set Vents to depressurise
                Airvent.Depressurise(m_airvents);

                // Wait for pressure to get to 0 - unless Oxygen tanks are full

                if ((m_airvents[0] as IMyAirVent).GetOxygenLevel() == 0 || m_oxyTanks.Count == 0 || oxyTankPercent() > 0.99)
                    m_state = STATE_OPENING;
            }
            else if (m_state == STATE_OPENING)
            {
                if (Door.OpenAndLockDoor(m_outerDoors))
                    m_state = STATE_IDLE;
            }
            else if (m_state == STATE_CLOSING)
            {
                if (Door.CloseAndLockDoor(m_outerDoors))
                    m_state = STATE_PRESSURISING;
            }
            else if (m_state == STATE_PRESSURISING)
            {
                Airvent.Pressurise(m_airvents);
                if ((m_airvents[0] as IMyAirVent).GetOxygenLevel() > 0.90 || m_oxyTanks.Count == 0 || oxyTankPercent() < 0.01)
                    m_state = STATE_UNSEALING;
            }
            else if (m_state == STATE_UNSEALING)
            {
                if (m_innerDoors.Count>0)
                {
                    Door.Open(m_innerDoors);
                    if (Door.IsOpen(m_innerDoors))
                        m_state = STATE_IDLE;
                } else
                    m_state = STATE_IDLE;
            }
            if (m_onUpdate!=null)
                m_onUpdate(m_airlockName, m_state, (m_airvents[0] as IMyAirVent).GetOxygenLevel());
        }
        private float oxyTankPercent()
        {
            float percentFull = 0;
            m_oxyTanks.ForEach(tank => { percentFull += (tank as IMyOxygenTank).GetOxygenLevel(); });
            percentFull = percentFull / m_oxyTanks.Count;
            return percentFull;
        }
    }
}