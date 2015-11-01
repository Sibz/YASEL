using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManager2
{
    using Grid;
    using Airvent;
    using Door;
    using Block;

    class AirlockManager
    {

        Dictionary<string, Airlock> m_airlocks;

        public Action<string, string, float> OnUpdate;

        public AirlockManager()
        {
            m_airlocks = new Dictionary<string, Airlock>();
        }

        public void Tick()
        {
            updateAirlocksTick();
        }

        public void AddAirlock(string airlockName, string outerDoorName, string airventName, string innerDoorName = "")
        {
            if (!m_airlocks.ContainsKey(airlockName))
                m_airlocks.Add(airlockName, new Airlock(airlockName, outerDoorName, innerDoorName, airventName, OnUpdate ));
            else
                m_airlocks[airventName] = new Airlock(airlockName, outerDoorName, innerDoorName, airventName, OnUpdate );
        }

        public void OpenAirlock(string airlockName)
        {
            if (m_airlocks.ContainsKey(airlockName))
                m_airlocks[airlockName].Open();
        }

        public void CloseAirlock(string airlockName)
        {
            if (m_airlocks.ContainsKey(airlockName))
                m_airlocks[airlockName].Close();
        }

        private void updateAirlocksTick()
        {
            var alEnum = m_airlocks.GetEnumerator();
            while (alEnum.MoveNext())
                alEnum.Current.Value.Tick();
        }
    }
    class Airlock
    {
        string m_airlockName, m_outerDoorName, m_innerDoorName, m_airventName;
        string m_state;

        List<IMyTerminalBlock> m_innerDoors;
        List<IMyTerminalBlock> m_outerDoors;
        List<IMyTerminalBlock> m_airvents;
        Action<string, string, float> m_onUpdate;

        public Airlock(string airlockName, string outerDoorName, string innerDoorName, string airventName, Action<string, string, float> m_onUpdate)
        {
            m_airlockName = airlockName;
            m_outerDoorName = outerDoorName;
            m_innerDoorName = innerDoorName;
            m_airventName = airventName;

            m_innerDoors = new List<IMyTerminalBlock>();
            m_outerDoors = new List<IMyTerminalBlock>();
            m_airvents = new List<IMyTerminalBlock>();

            if (m_innerDoorName!="")
                Grid.ts.GetBlocksOfType<IMyDoor>(m_innerDoors, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(m_innerDoorName) && Grid.BelongsToGrid(b));});
            Grid.ts.GetBlocksOfType<IMyDoor>(m_outerDoors, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(outerDoorName) && Grid.BelongsToGrid(b)); });
            Grid.ts.GetBlocksOfType<IMyAirVent>(m_airvents, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(m_airventName) && Grid.BelongsToGrid(b)); });
            if (m_outerDoors.Count == 0 || m_airvents.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + m_airlockName + "' - Outer Doors or Vents not found");
        }

        public void Open()
        {
            m_state = "opening";
        }
        public void Close()
        {
            m_state = "closing";
        }
        public void Tick()
        {
            if (m_state=="opening")
            {
                // If we have inner doors close them
                if (m_innerDoors.Count>0)
                    Door.Close(m_innerDoors);
                // Set Vents to depressurise
                Airvent.Depressurise(m_airvents);
                if ((m_airvents[0] as IMyAirVent).GetOxygenLevel() == 0)
                    m_state = "opening-2";
            }
            else if (m_state == "opening-2")
            {
                // OpenAndLock outer Doors
                if (Door.OpenAndLockDoor(m_innerDoors))
                    m_state = "idle";
            }
        }
    }
}