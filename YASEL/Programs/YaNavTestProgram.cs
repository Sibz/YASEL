using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;


namespace YaNavTestProgram
{
    using YaNavControl;
    using YaNavGyroControl;
    using YaNavThrusterControl;
    using ProgramExtensions;
    using GyroExtensions;
    using InventoryExtensions;
    using ConnectorExtensions;
    using BlockExtensions;

    class YaNavTestProgram : MyGridProgram
    {

        YaNavControl navController;
        int ticks = 0;
        int ticksPerRun = 15;
        /*
        string connectorName = "Drill Ship Connector";
        float StoppingDistanceMin = 850,
            StoppingDistanceMax = 1500,
            MassCoEfMin = 0.8f,
            MassCoEfMax = 1.7f;
       */
        string connectorName = "Ship Connector";
        float StoppingDistanceMin = 450,
        StoppingDistanceMax = 800,
        MassCoEfMin = 0.5f,
        MassCoEfMax = 0.8f;

        IMyShipConnector connector;
        List<IMyInventory> cargoInventories = new List<IMyInventory>();
        IMyTextPanel tp;
        
        string tpName = "LCD Info";

        void Main(string argument)
        {

            if (navController == null)
            {
                connector = this.GetBlock(connectorName) as IMyShipConnector;
                tp = (this.GetBlock(tpName) as IMyTextPanel);
                cargoInventories = this.GetInventories();
                List<IMyTerminalBlock> gyros = new List<IMyTerminalBlock>();
                this.GridTerminalSystem.GetBlocksOfType<IMyGyro>(gyros, this.OnGrid);
                navController = new YaNavControl(this, new YaNavSettings()
                {
                    Remote = this.GetBlock("Remote") as IMyRemoteControl,
                    TickCount = ticksPerRun,
                    Debug = new List<string>() { "tick", "initControl", "initThruster", "travelProcess", "move" },
                    GyroSettings = new YaNavGyroControlSettings() { Gyroscopes = gyros, GyroCoEff = 0.5f },
                    StoppingDistance = StoppingDistanceMin + ((StoppingDistanceMax - StoppingDistanceMin) * cargoInventories.GetPercentFull()),
                    ThrusterSettings = new YaNavThrusterSettings() { MassCoEff = 0.9f }

                });
            }
            if (string.IsNullOrEmpty(argument))
            {
                navController.Settings.StoppingDistance = StoppingDistanceMin + ((StoppingDistanceMax - StoppingDistanceMin) * cargoInventories.GetPercentFull());
                navController.Settings.ThrusterSettings.MassCoEff = MassCoEfMin + ((MassCoEfMax - MassCoEfMin) * cargoInventories.GetPercentFull());
            }
            if (!string.IsNullOrEmpty(argument))
                switch (argument)
                {
                    case "rtb":
                        rtb();
                        break;
                    case "enter":
                        enterBase();
                        break;
                    case "exit":
                        exitBase();
                        break;
                    case "park":
                        park();
                        break;
                    case "unpark":
                        unPark();
                        break;
                    case "parkWelder":
                        parkWelder();
                        break;
                    case "parkAndReturn":
                        parkAndReturn();
                        break;
                    case "stop":
                    default:
                        navController.StopAndClear();
                        break;

                }
            if (ticks % ticksPerRun == 0)
            {
                navController.Tick();
                var pos = navController.Settings.Remote.GetPosition();
                if (tp != null) tp.WritePublicText(Math.Round(pos.X, 2) + "," + Math.Round(pos.Y, 2) + "," + Math.Round(pos.Z, 2));
                Matrix m = new Matrix();
                navController.Settings.Remote.Orientation.GetMatrix(out m);
                pos = Vector3.Negate(Vector3.Transform(navController.Settings.Remote.GetDirectionalVector(), navController.Settings.Remote.WorldMatrix.GetOrientation()));
                //pos = Vector3.Normalize(navController.Settings.Remote.GetDirectionalVector());
                if (tp != null) tp.WritePublicText("\n" + Math.Round(pos.X, 2) + "," + Math.Round(pos.Y, 2) + "," + Math.Round(pos.Z, 2), true);
            }
            ticks++;

        }

        private void parkWelder()
        {
            var pointTowards = (new Vector3D(-0.72, 0.37, 0.59));
            //27474.28,59736.36,-4575.45
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27474.28, 59736.36, -4575.45),
                Speed = 15f,
                Precision = 2.5f,
                OrientateToNormalizedVector = pointTowards
            });
            //27472.18,59731.56,-4574.94
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27472.18, 59731.56, -4574.94),
                Speed = 3f,
                Precision = 0.15f,
                OrientateToNormalizedVector = pointTowards,
                ResetThrusters = true
            });
            navController.AddTask(new YaNavWaitTask() { OnComplete = connector.Lock });
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27472.18, 59731.56, -4574.94),
                ResetThrusters = true
            });
            navController.AddTask(new YaNavWaitTask() { Milliseconds = 2000, Hover = false });
          
        }
        private void parkAndReturn()
        {
            parkWelder();
            unparkWelder();
            returnToLoc();
        }
        private void unparkWelder()
        {
            var pointTowards = (new Vector3D(-0.72, 0.37, 0.59));
            navController.AddTask(new YaNavWaitTask()
            {
                OnComplete = connector.UnLock,
                Milliseconds = 300
            });
            navController.AddTask(new YaNavWaitTask()
            {
                OnComplete = connector.TurnOff,
                Milliseconds = 300
            });
            //27474.28,59736.36,-4575.45

            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27474.28, 59736.36, -4575.45),
                Speed = 10f,
                Precision = 0.5f,
                OrientateToNormalizedVector = pointTowards,
                OnComplete = connector.TurnOn,
            });


        }

        private void returnToLoc()
        {
            navController.AddTask(new YaNavTravelTask()
            {
                Target = navController.Settings.Remote.GetPosition(),
                Speed = 20f,
                Precision = 3f,
                ResetThrusters = true
            });
        }

        private void silver()
        {
            //GPS: Ag Co:24633.82:59135.7:-6609.83:
            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(24633.82, 59135.7, -6609.83), Speed = 95f, Precision = 100f, OrientateFirst = true });
        }
        private void park()
        {
            //GPS:Sibz #2:27462.16:59737.21:-4550.91:
            var pointTowards = (new Vector3D(-0.72, 0.37, 0.59));

            //GPS:ParkBayOre-1:27487.44:59723.95:-4571.71:
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27487.79, 59724.69, -4571.46),
                Speed = 90f,
                Precision = 3f,
                SpeedAtTarget = 1f,
                OrientateFirst = true,
                OrientateToNormalizedVector = pointTowards
            });

            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27479.19, 59729.17, -4564.82),
                Speed = 5f,
                Precision = 0.25f,
                OrientateFirst = false,
                OrientateToNormalizedVector = pointTowards
            });
            //GPS: ParkBayOre - 2:27479.46:59727.85:-4565.35:
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27478.58, 59727.83, -4564.72),
                Speed = 3f,
                Precision = 0.15f,
                OrientateFirst = false,
                OrientateToNormalizedVector = pointTowards,
                ResetThrusters = true
            });
            navController.AddTask(new YaNavWaitTask() { OnComplete = connector.Lock, Hover = true });
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27478.58, 59727.83, -4564.72),
                ResetThrusters = true
            });
        }
        private void unPark()
        {
            navController.AddTask(new YaNavWaitTask()
            {
                OnComplete = connector.UnLock,
            });
            navController.AddTask(new YaNavWaitTask()
            {
                OnComplete = connector.TurnOff,
            });
            navController.AddTask(new YaNavWaitTask()
            {
            });
            var pointTowards = new Vector3D(-0.72, 0.37, 0.59);
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27480.85, 59730.04, -4565.8),
                Speed = 5f,
                SpeedAtTarget = 0.25f,
                Precision = 1f,
                OrientateFirst = false,
                OnComplete = connector.TurnOn,
                OrientateToNormalizedVector = pointTowards
            });
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27492.87, 59722.88, -4576.87),
                Speed = 5f,
                SpeedAtTarget = 1f,
                Precision = 3f,
                OrientateFirst = false,

                OrientateToNormalizedVector = pointTowards
            });
        }

        private void enterBase()
        {
            //GPS:ToBase2:27544.04:59702.39:-4516.77:
            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27551.72, 59700, -4507.75), Speed = 90f, SpeedAtTarget = 5f, Precision = 5f, OrientateFirst = false });
            //GPS:ToBase3:27492.98:59719.4:-4573.84:
            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27509.2, 59715.47, -4556.3), Speed = 90f, SpeedAtTarget = 7.5f, Precision = 3f, OrientateFirst = false });

            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27494.35, 59721.15, -4575.91), Speed = 90f, SpeedAtTarget = 0f, Precision = 1f, OrientateFirst = false });
        }
        private void exitBase()
        {
            //navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27492.98, 59719.4, -4573.84), Speed = 90f, SpeedAtTarget = 3f, Precision = 5f, OrientateFirst = true, OrientateTo = new Vector3D(27544.04, 59702.39, -4516.77) });
            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27492.98, 59719.4, -4573.84), Speed = 5f, SpeedAtTarget = 0.5f, Precision = 1f, OrientateFirst = true, OrientateTo = new Vector3D(27544.04, 59702.39, -4516.77) });

            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27544.04, 59702.39, -4516.77),
                Speed = 100f,
                Precision = 20f,
                OrientateFirst = true,
                SlowForTarget = false,
            });
            //GPS: Sibz #3:27989.44:59235.96:-4110.04:
            navController.AddTask(new YaNavTravelTask()
            {
                Target = new Vector3D(27989.44, 59235.96, -4110.04),
                Speed = 90f,
                Precision = 200f,
                OrientateFirst = false,
                SlowForTarget = true,
            });
        }

        private void rtb()
        {
            //GPS:Sibz #1:28405.06:59601.02:-5115.67:
            //navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(28405.06, 59601.02, -5115.67), Speed = 95f, Precision = 400f });
            //GPS:ToBase1:27601.22:59684.3:-4439.93:
            navController.AddTask(new YaNavTravelTask() { Target = new Vector3D(27601.22, 59684.3, -4439.93), Speed = 90f, Precision = 20f });
        }
    }
}

