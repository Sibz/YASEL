using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavControl
{
    using ProgramExtensions;
    using ThrustExtensions;
    using BlockExtensions;
    using YaNavThrusterControl;
    using GyroExtensions;
    using YaNavGyroControl;
    using YaNav;

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
            if (tasks.Count>0)
            {
                if (tasks[0] is YaNavTravelTask) (tasks[0] as YaNavTravelTask).Process();
                if (tasks[0].Complete)
                {
                    tasks.Remove(tasks[0]);
                }
            } else
            {
                Settings.Remote.SetValueBool("DampenersOverride", true);
            }
        }
        public void AddTask(YaNavTask task)
        {
            task.AddControllerVars(gp, this);
            tasks.Add(task);
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

    public class YaNavTravelTask : YaNavTask
    {
        public Vector3D? Target = null;
        public float Speed = 100f;
        public float Precision = 10f;
        public bool OrientateFirst = false;
        public bool SlowForTarget = true;
        public Vector3D? OrientateTo = null;
        public Vector3D? OrientationIndicator = null;

        private bool orientateToTarget = false;
        private bool orientated = false;
        private bool firstRun = true;

        public override void Process()
        {
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("checking target");
            if (!Target.HasValue)
            {
                this.complete();
                return;
            }
            Target = navController.Settings.Remote.GetFreeDestination(Target.Value, 1000000f, 10f);
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("calculating difference");
            var difference = Target.Value - navController.Settings.Remote.GetPosition();
            if (difference.Length()<Precision)
            {
                this.complete();
                return;
            }
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("calculating speed vars: len" + difference.Length());
            float adjustedSpeed;
            if (SlowForTarget)
            {
                float speedPercent = Speed / navController.Settings.MaxSpeed;
                float adjustedStoppingDistance = speedPercent * navController.Settings.StoppingDistance;
                if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("adjustedStoppingDistance " + adjustedStoppingDistance);
                adjustedSpeed = (float)(adjustedStoppingDistance < difference.Length() + Precision ? Speed : Speed * (difference.Length() / adjustedStoppingDistance));
            } else 
                adjustedSpeed = Speed;


            if (!OrientateTo.HasValue)
                orientateToTarget = true;
            if (orientateToTarget)
                OrientateTo = Vector3D.Normalize(Target.Value - navController.Settings.Remote.GetPosition());

            // If no indicator given, use remotes forward vector.
            if (!OrientationIndicator.HasValue)
            {
                OrientationIndicator = navController.Settings.Remote.GetDirectionalVector();
            }
            if (navController.Settings.Debug.Contains("travelProcess")) gp.Echo("moving forward @ " + adjustedSpeed + " m/s");
            navController.GyroscopeController.SetVectorAndDirection(OrientateTo.Value);


            if (!firstRun)
                orientated |= !navController.GyroscopeController.IsRotating;
            else
                firstRun = false;
            
            var localAngle = Vector3D.Transform(Target.Value - navController.Settings.Remote.GetPosition(), MatrixD.Transpose(navController.Settings.Remote.WorldMatrix.GetOrientation()));

            if ((OrientateFirst && orientated) || !OrientateFirst) navController.ThrusterController.MoveAngle(localAngle, adjustedSpeed);
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
            navController.ThrusterController.StopThrusters();
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