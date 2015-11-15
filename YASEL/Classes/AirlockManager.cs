using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace AirlockManager
{
    using AirventManager;
    using DoorManager;
    using BlockExtensions;

    class AirlockManager
    {
        Dictionary<string, AirlockNP> airlocks;
        MyGridProgram gp;

        public Action<string, string, float> OnUpdate;

        public AirlockManager(MyGridProgram gp, Action<string, string, float> onUpdate = null)
        {
            this.gp = gp;
            airlocks = new Dictionary<string, AirlockNP>();
            if (onUpdate != null)
                OnUpdate = onUpdate;
        }

        public void Tick()
        {
            var alEnum = airlocks.GetEnumerator();
            while (alEnum.MoveNext())
                alEnum.Current.Value.Tick();
        }

        public void AddAirlockNP(string airlockName, string outerDoorName, string innerDoorName)
        {
            var newAirlock = new AirlockNP(gp, airlockName, outerDoorName, innerDoorName, OnUpdate);
            if (!airlocks.ContainsKey(airlockName))
                airlocks.Add(airlockName, newAirlock);
            else
                airlocks[airlockName] = newAirlock;
        }

        public void AddAirlock(string airlockName, string outerDoorName, string airventName, string innerDoorName = "")
        {
            var newAirlock = new Airlock(gp, airlockName, outerDoorName, innerDoorName, airventName, OnUpdate );
            if (!airlocks.ContainsKey(airlockName))
                airlocks.Add(airlockName, newAirlock);
            else
                airlocks[airlockName] = newAirlock;
        }

        public void OpenAirlock(string airlockName)
        {
            if (airlocks.ContainsKey(airlockName))
                airlocks[airlockName].Open();
            else
                throw new Exception("AirlockManager: Unable to open airlock, name not found");
        }

        public void CloseAirlock(string airlockName)
        {
            if (airlocks.ContainsKey(airlockName))
                airlocks[airlockName].Close();
            else
                throw new Exception("AirlockManager: Unable to close airlock, name not found");
        }

    }

    class AirlockNP
    {
        MyGridProgram gp;

        protected string airlockName, outerDoorName, innerDoorName;

        protected DoorManager innerDoorManager;
        protected DoorManager outerDoorManager;
        
        protected Action<string, string, float> onUpdate;

        protected const string Sealing = "Sealing";
        protected const string Opening = "Opening";
        protected const string Closing = "Closing";
        protected const string Unsealing = "Unsealing";
        protected const string Idle = "Idle";

        protected string state = Idle;

        public AirlockNP(MyGridProgram gp, string airlockName, string outerDoorName, string innerDoorName, Action<string, string, float> onUpdate)
        {
            this.airlockName = airlockName;
            this.outerDoorName = outerDoorName;
            this.innerDoorName = innerDoorName;
            this.onUpdate = onUpdate;
            innerDoorManager = new DoorManager();
            outerDoorManager = new DoorManager();

            if (innerDoorName != "")
                gp.GridTerminalSystem.GetBlocksOfType<IMyDoor>(innerDoorManager.Doors, b => { return (b.CustomName.Contains(innerDoorName) && b.CubeGrid==gp.Me.CubeGrid); });
            gp.GridTerminalSystem.GetBlocksOfType<IMyDoor>(outerDoorManager.Doors, b => { return (b.CustomName.Contains(outerDoorName) && b.CubeGrid==gp.Me.CubeGrid); });

            if (outerDoorManager.Doors.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + airlockName + "' -  Outer doors not found");
        }

        public void Open()
        {
            state = Sealing;
        }
        public void Close()
        {
            state = Closing;
        }
        public virtual void Tick()
        {
            if (innerDoorManager.Doors.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + airlockName + "' -  Inner doors not found");
            if (state == Sealing)
            {
                if (innerDoorManager.CloseAndLock())
                    state = Opening;
            }
            else if (state == Opening)
            {

                if (!innerDoorManager.AreAllClosed())
                {
                    state = Sealing;
                    return;
                }
                if (outerDoorManager.OpenAndLock())
                    state = Idle;
            }
            else if (state == Closing)
            {
                if (outerDoorManager.CloseAndLock())
                    state = Unsealing;
            }
            else if (state == Unsealing)
            {
                if (!outerDoorManager.AreAllClosed())
                {
                    state = Closing;
                    return;
                }
                if (innerDoorManager.OpenAndLock())
                    state = Idle;
            }
            if (onUpdate != null)
                onUpdate(airlockName, state, 0);
        }
    }

    class Airlock : AirlockNP
    {
        string airventName;

        const string Depressurising = "Depressurising";
        const string Pressurising = "Pressurising";


        AirventManager airlockVentManager;
        List<IMyTerminalBlock> oxyTanks;
        

        public Airlock(MyGridProgram gp, string airlockName, string outerDoorName, string innerDoorName, string airventName, Action<string, string, float> onUpdate) :
            base(gp, airlockName, outerDoorName, innerDoorName, onUpdate)
        {
            this.airventName = airventName;

            airlockVentManager = new AirventManager();
            oxyTanks = new List<IMyTerminalBlock>();

            this.onUpdate = onUpdate;

            gp.GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(oxyTanks, b => { return b.CubeGrid == gp.Me.CubeGrid; });

            gp.GridTerminalSystem.GetBlocksOfType<IMyAirVent>(airlockVentManager.Airvents, delegate(IMyTerminalBlock b) { return (b.CustomName.Contains(airventName) && b.CubeGrid == gp.Me.CubeGrid); });
            if (airlockVentManager.Airvents.Count == 0)
                throw new Exception("Airlock Error: Unable to initialise airlock '" + airlockName + "' -  Vents not found");
        }


        public override void Tick()
        {
            if (state == Sealing)
            {
                // If we have inner doors, close them
                if (innerDoorManager.Doors.Count>0)
                    innerDoorManager.Shut();
                state = Depressurising;
            }
            else if (state == Depressurising)
            {
                // Set Vents to depressurise
                airlockVentManager.Depressurise();

                // Wait for pressure to get to 0 - unless Oxygen tanks are full

                if ((airlockVentManager.Airvents[0] as IMyAirVent).GetOxygenLevel() == 0 || oxyTanks.Count == 0 || oxyTankPercent() > 0.99)
                    state = Opening;
            }
            else if (state == Opening)
            {
                if (outerDoorManager.OpenAndLock())
                    state = Idle;
            }
            else if (state == Closing)
            {
                if (outerDoorManager.CloseAndLock())
                    state = Pressurising;
            }
            else if (state == Pressurising)
            {
                airlockVentManager.Pressurise();
                if ((airlockVentManager.Airvents[0] as IMyAirVent).GetOxygenLevel() > 0.90 || oxyTanks.Count == 0 || oxyTankPercent() < 0.01)
                    state = Unsealing;
            }
            else if (state == Unsealing)
            {
                if (innerDoorManager.Doors.Count>0)
                {
                    innerDoorManager.Open();
                    if (innerDoorManager.AreAllOpen())
                        state = Idle;
                } else
                    state = Idle;
            }
            if (onUpdate!=null)
                onUpdate(airlockName, state, (airlockVentManager.Airvents[0] as IMyAirVent).GetOxygenLevel());
        }
        private float oxyTankPercent()
        {
            float percentFull = 0;
            oxyTanks.ForEach(tank => { percentFull += (tank as IMyOxygenTank).GetOxygenLevel(); });
            percentFull = percentFull / oxyTanks.Count;
            return percentFull;
        }
    }
}