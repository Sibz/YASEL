using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using Sandbox.Game.Entities;

namespace YaNavControl
{
    using ProgramExtensions;
    using ThrustExtensions;
    using BlockExtensions;
    using YaNavThrusterControl;
    using GyroExtensions;
    using YaNavGyroControl;
    using YaNav;
    using RemoteExtensions;

    public class YaNavControl
    {
        //Class Variables
        MyGridProgram gp;
        public YaNavSettings Settings;


        //Controllers
        public YaNavGyroControl GyroscopeController;
        public YaNavThrusterControl ThrusterController;

        //Waypoint Storage
        List<YaNavTask> tasks;

        public YaNavControl(MyGridProgram gp, YaNavSettings s)
        {
            this.gp = gp;
            Settings = s;
            Settings.InitThrusterSettings();
            Settings.InitGyroSettings();

            if (Settings.Debug.Contains("initControl")) gp.Echo("Initialising YaNav");

            tasks = new List<YaNavTask>();
            GyroscopeController = new YaNavGyroControl(gp, Settings.GyroSettings);
            if (Settings.Remote == null)
                throw new YaNavException(lang.ErrorNullRemote);
            ThrusterController = new YaNavThrusterControl(gp, Settings.ThrusterSettings);

            if (Settings.Debug.Contains("initControl")) gp.Echo("Initialisation complete");
            //var testTask = new YaNavTravelTask();


        }
        public void Tick()
        {
            ThrusterController.Tick();
            GyroscopeController.Tick();
            if (tasks.Count > 0)
            {
                 if (Settings.Remote.DampenersOverride)
                    Settings.Remote.SetValueBool("DampenersOverride", false);
                //if (tasks[0] is YaNavTravelTask) (tasks[0] as YaNavTravelTask).Process();
                tasks[0].Process();
                if (tasks[0].Complete)
                {
                    tasks.Remove(tasks[0]);
                }
            }
            else if (!Settings.Remote.DampenersOverride)
                Settings.Remote.SetValueBool("DampenersOverride", true);

        }
        public void AddTask(YaNavTask task)
        {
            task.AddControllerVars(gp, this);
            tasks.Add(task);
        }
        /// <summary>
        /// Stops Gyro/Thruster movement
        /// </summary>
        public void StopAndClear()
        {
            tasks.Clear();
            GyroscopeController.ClearVectorAndDirection();
            Settings.Remote.SetValueBool("DampenersOverride", true);
            ThrusterController.StopThrusters();
        }

        private void travel(YaNavTravelTask task)
        {

        }

    }

    public abstract class YaNavTask
    {
        public bool Complete = false;
        public Action OnComplete;
        public abstract void Process();
        protected MyGridProgram gp;
        protected YaNavControl navController;
        public void AddControllerVars(MyGridProgram gp, YaNavControl navController)
        {
            this.gp = gp;
            this.navController = navController;
        }
        protected virtual void complete()
        {
            Complete = true;
            if (OnComplete != null)
                OnComplete();
        }
    }
    public class YaNavWaitTask : YaNavTask
    {
        public int Milliseconds = 1000;
        public bool Hover = true;

        private DateTime? startTime = null;
        public override void Process()
        {
            if (!startTime.HasValue)
                startTime = DateTime.Now;
            if (startTime.Value.AddMilliseconds(Milliseconds) < DateTime.Now)
            {
                complete();
                return;
            }
            if (Hover)
            {
                navController.ThrusterController.MoveForward(0f);
                navController.ThrusterController.MoveUp(0f);
                navController.ThrusterController.MoveLeft(0f);
            }
        }
    }
    public class YaNavTravelTaskRemote : YaNavTask
    {
        public Vector3D? Target = null;
        public bool Precision = false;
        public Base6Directions.Direction Direction = Base6Directions.Direction.Forward;
        private bool started = false;


        public override void Process()
        {
            var remote = navController.Settings.Remote;
            if (!started && !remote.IsAutoPilotEnabled)
            {
                remote.ClearWaypoints();
                remote.AddWaypoint(Target.Value, "new");
                remote.SetValueBool("DockingMode", Precision);
                remote.SetDirection(Direction);
                var actions = new List<ITerminalAction>();
                remote.GetActions(actions);
                foreach (var ac in actions)
                    gp.Echo(ac.Id + ":" + ac.Name);
                remote.SetAutoPilotEnabled(true);
                started = true;
            }
            else if (started && !remote.IsAutoPilotEnabled)
            {
                complete();
                return;
            }
        }
    }
    public class YaNavTravelTask : YaNavTask
    {
        /// <summary>
        /// The target to travel to
        /// </summary>
        public Vector3D? Target = null;
        /// <summary>
        /// Max speed to try achieve
        /// </summary>
        public float Speed = 100f;
        /// <summary>
        /// How close to a waypoint is considered as there
        /// </summary>
        public float Precision = 10f;
        /// <summary>
        /// Stop and orientate towards target first. 
        /// A must if navigating around tight spaces
        /// </summary>
        public bool OrientateFirst = false;
        /// <summary>
        /// If accuracy isn't important and speed is, you can set this to false
        /// Usefuly for moving quickly between multiple waypoints
        /// </summary>
        public bool SlowForTarget = true;
        /// <summary>
        /// Set true to ensure the thrusters are turned off when destination is reached
        /// Intended for last waypoint in a movement
        /// </summary>
        public bool ResetThrusters = false;
        //public bool CollisionDetection = true;
        /// <summary>
        /// What vector to look at when moving around, left null and you look at your target
        /// </summary>
        public Vector3D? OrientateTo = null;
        /// <summary>
        /// To use a pre determined orientatation
        /// </summary>
        public Vector3D? OrientateToNormalizedVector = null;
        /// <summary>
        /// What vector to use to go 'forward' 
        /// Helper function (block)GetDirectionalVector() from gyroextensions can be used
        /// i.e. remote.GetDirectionalVector("down")
        /// </summary>
        public Vector3D? OrientationIndicator = null;
        /// <summary>
        /// Speed to be at the target
        /// </summary>
        public float SpeedAtTarget = 0.1f;

        //private bool orientateToTarget = false;
        private bool orientated = false;
        private bool firstRun = true;
        private Vector3D orientateTo;
        private Vector3D orientationIndicator;

        public override void Process()
        {
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("checking target");
            if (!Target.HasValue)
            {
                this.complete();
                return;
            }
            // No longer going to be supported
            //if (CollisionDetection) Target = navController.Settings.Remote.GetFreeDestination(Target.Value, 1000000f, 10f);
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("calculating difference");
            // get difference betwe us and target
            var difference = Target.Value - navController.Settings.Remote.GetPosition();
            // if length is less than precision then we are there
            if (difference.Length() < Precision)
            {
                this.complete();
                return;
            }
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("calculating speed vars: len" + difference.Length());
            float adjustedSpeed;

            if (SlowForTarget)
            {
                float speedPercent = Speed / navController.Settings.MaxSpeed;
                float adjustedStoppingDistance = (speedPercent * navController.Settings.StoppingDistance) - Precision;
                // Do something with precision, the higher the number the less we should consider the stopping distance
                adjustedStoppingDistance = adjustedStoppingDistance * (1f - ((Precision / adjustedStoppingDistance) / 2));

                if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("adjustedStoppingDistance " + adjustedStoppingDistance);

                float percentIntoStoppingArea = 1 - ((float)difference.Length() / adjustedStoppingDistance);

                // adjust the speed for speed we want to be at the target
                var speed = Speed - SpeedAtTarget;

                if (percentIntoStoppingArea > 0f)
                {
                    adjustedSpeed = (float)(adjustedStoppingDistance < difference.Length() ? Speed : Speed * ((difference.Length()) / adjustedStoppingDistance));

                    /*
                    // in the first 25% slow quickly  / 45% speed reduction here1
                    if (percentIntoStoppingArea < 0.25f)
                    {
                        // speed - ( how many percent through the first 25%) of 45% 

                        float percentThrough = percentIntoStoppingArea * 4;
                        adjustedSpeed = speed - ((speed * 0.45f) * percentThrough);
                    }
                    // in the middle 50% slow slowly / 30% speed reduction here
                    else if (percentIntoStoppingArea < 0.75f)
                    {
                        float percentThrough = (percentIntoStoppingArea - 0.25f) * 2;
                        adjustedSpeed = speed * 0.55f - (speed * 0.30f * percentThrough);
                    }
                    // in the last 25% slow quickly / 25% speed reduction here
                    else
                    {
                        float percentThrough = (percentIntoStoppingArea - 0.75f) * 4;
                        adjustedSpeed = speed * 0.25f - (speed * 0.25f * percentThrough);
                    }
                    */
                    //double precisionPercent = Precision / adjustedStoppingDistance;
                }
                else
                    adjustedSpeed = speed;

                adjustedSpeed = adjustedSpeed + SpeedAtTarget;
            }
            else
                adjustedSpeed = Speed;

            // If no orientation specified, orientate towards target 
            // (otherwise we'll orientate the ship in specified direction and move towards target, i.e. face 'forward', move 'left')
            if (OrientateTo.HasValue)
                orientateTo = Vector3D.Normalize(OrientateTo.Value - navController.Settings.Remote.GetPosition());
            else if (OrientateToNormalizedVector.HasValue)
                orientateTo = OrientateToNormalizedVector.Value;
            else
                orientateTo = Vector3D.Normalize(Target.Value - navController.Settings.Remote.GetPosition());

            // If no indicator given, use remotes forward vector.
            if (!OrientationIndicator.HasValue)
                orientationIndicator = navController.Settings.Remote.GetDirectionalVector();

            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("moving forward @ " + adjustedSpeed + " m/s");

            navController.GyroscopeController.SetTargetAndIndicator(orientateTo, orientationIndicator);

            // Will not be rotating on first run
            if (!firstRun)
                orientated |= !navController.GyroscopeController.IsRotating;
            else
                firstRun = false;

            var localAngle = Vector3D.Transform(Target.Value - navController.Settings.Remote.GetPosition(), MatrixD.Transpose(navController.Settings.Remote.WorldMatrix.GetOrientation()));
            var currentAngle = Vector3.Transform(orientationIndicator, MatrixD.Transpose(navController.Settings.Remote.WorldMatrix.GetOrientation()));

            // Only move at full speed if we are facing the desired vectory

            gp.Echo("Angle:" + navController.GyroscopeController.Angle);


            var angleSpeedAdjustment = (1 - Math.Min(navController.GyroscopeController.Angle, 1)) / 2;
            //navController.GyroscopeController.Angle < 0.25f && navController.GyroscopeController.Angle > -0.25f && 
            if (angleSpeedAdjustment > 0.01f && ((OrientateFirst && orientated) || !OrientateFirst))         // SPEED must be a min
                navController.ThrusterController.MoveAngle(localAngle, Math.Max(0.15f, adjustedSpeed * (angleSpeedAdjustment + 0.5f)));

            else
            {
                navController.ThrusterController.MoveForward(0f);
                navController.ThrusterController.MoveUp(0f);
                navController.ThrusterController.MoveLeft(0f);
            }





        }

        protected override void complete()
        {
            navController.GyroscopeController.ClearVectorAndDirection();
            if (ResetThrusters) navController.ThrusterController.StopThrusters();
            base.complete();
        }

    }/*
    public class YaNavConnectTask : YaNavTask
    {
        public Vector3D ShipLocationToConnect; //Where the should be to connect
        public Vector3D ShipOrientationToConnect; // What the orientation the ship needs to connect
        public bool PowerDownConnector = true; // Power down the connector when trying to connect

    }
    public class YaNavWaitTask : YaNavTask
    {
        public float TimeOut = 10f;
        public bool Hover = true;
        public bool MaintainHeading = true;
        public bool CutEngines = false;
    }
    public class YaNavWaitForRecharge : YaNavWaitTask
    {
        public float TimeOut = 900f; // Timeout after 15mins
        public bool SetBatteriesToRecharge = true; // Initially set batteries to recharge
        public bool SetBatteriesToDischarge = true; // After recharged or timeout, set to Discharge
        public bool Hover = false;
        public bool MaintainHeading = false;
        public bool CutEngines = true;
    }
    public class YaNavWaitUntilEmpty : YaNavWaitTask
    {
        public float TimeOut = 60f; // Timeout after 1mins
        public bool Hover = false;
        public bool MaintainHeading = false;
        public bool CutEngines = true;
    }*/

    public class YaNavSettings
    {
        public IMyRemoteControl Remote;
        public List<string> Debug;
        public YaNavThrusterSettings ThrusterSettings;
        public YaNavGyroControlSettings GyroSettings;
        public int TickCount = 15;
        public float MaxSpeed = 100f;
        public float StoppingDistance = 1000f; // how far @ max speed to start slowing down

        public YaNavSettings()
        {
            Debug = new List<string>();
            ThrusterSettings = new YaNavThrusterSettings();
            GyroSettings = new YaNavGyroControlSettings();
        }
        public void InitThrusterSettings()
        {
            ThrusterSettings.Debug = this.Debug;
            ThrusterSettings.Remote = this.Remote;
            ThrusterSettings.TickCount = this.TickCount;
            ThrusterSettings.MaxSpeed = this.MaxSpeed;
        }
        public void InitGyroSettings()
        {
            GyroSettings.Remote = this.Remote;
            GyroSettings.OrientationReferenceBlock = this.Remote;
        }


    }

}