using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavThrusterControl
{
    using YaNav;
    using BlockExtensions;
    using ThrustExtensions;
    using ProgramExtensions;

    public class YaNavThrusterControl
    {
        MyGridProgram gp;
        YaNavThrusters thrusters;
        Vector3D lastPosition;
        float currentSpeed, time, tickTime;
        YaNavThrusterVars forward, right, up;
        YaNavThrusterSettings settings;

        public YaNavThrusterControl(MyGridProgram gp, YaNavThrusterSettings settings)
        {
            // SEt Class Variables
            this.gp = gp; // MyGridProgram
            this.settings = settings;
            
            // Set up thruster groups
            thrusters = new YaNavThrusters(gp, settings.thrusterGroupNames);
            if (thrusters.Forward.Count == 0 ||
                thrusters.Backward.Count == 0 ||
                thrusters.Left.Count == 0 ||
                thrusters.Right.Count == 0 ||
                thrusters.Up.Count == 0)
                throw new YaNavThrusterControlException(lang.ErrorNoThrusterBlocks);
          
            if (thrusters.Down.Count == 0 && !settings.inNatrualGravityOnly)
                throw new YaNavThrusterControlException(lang.ErrorNoDownThrusters);

            // Variable stores for each thrust axis
            forward = new YaNavThrusterVars() { PositiveThrusters = thrusters.Forward, NegativeThrusters = thrusters.Backward };
            right = new YaNavThrusterVars() { PositiveThrusters = thrusters.Right, NegativeThrusters = thrusters.Left };
            up = new YaNavThrusterVars() { PositiveThrusters = thrusters.Up, NegativeThrusters = thrusters.Down };
            
            // Set last position
            lastPosition = settings.positionReferenceBlock.GetPosition();

            // Calculate the game time per run
            tickTime = 1f * ((float)settings.tickCount / 60f);
           
 
        }
        public void Tick()
        {

            // Get the change in position
            Vector3 changeInPosition = settings.positionReferenceBlock.GetPosition() - lastPosition;
            // reset last position
            lastPosition = settings.positionReferenceBlock.GetPosition();

            // Calculate overall speed            
            currentSpeed = changeInPosition.Length() / tickTime;

            // Calculate speeds(and acceleration for each axis)
            forward.SetSpeeds(Vector3.Dot(changeInPosition, settings.positionReferenceBlock.WorldMatrix.Forward), tickTime);
            right.SetSpeeds(Vector3.Dot(changeInPosition, settings.positionReferenceBlock.WorldMatrix.Right), tickTime);
            up.SetSpeeds(Vector3.Dot(changeInPosition, settings.positionReferenceBlock.WorldMatrix.Up), tickTime);

            // Debug info
            /*
            gp.Echo("Speed:" + currentSpeed + " m/s" + "\n" +
            "\n Forward: " + forward.Speed +
            "\n Right: " + right.Speed +
                "\n Up: " + up.Speed +
                "\n AccelForward:" + forward.Acceleration +
                "\n AccelRight:" + right.Acceleration +
                "\n AccelUp:" + up.Acceleration +
            "\net: " + gp.ElapsedTime.Milliseconds.ToString());*/
        }
        public void MoveForward(float targetSpeed = 50)
        {
            move(targetSpeed);
        }
        public void MoveBackward(float targetSpeed = 50)
        {
            move(-targetSpeed);
        }
        public void MoveRight(float targetSpeed = 50)
        {
            move(targetSpeed, right);
        }
        public void MoveLeft(float targetSpeed = 50)
        {
            move(-targetSpeed, right);
        }
        
        public void MoveUp(float targetSpeed = 50)
        {
            move(targetSpeed, up);
        }
        public void MoveDown(float targetSpeed = 50)
        {
            move(-targetSpeed, up);
        }
        
        private void move(float targetSpeed = 50, YaNavThrusterVars thrusterVars = null)
        {
            if (thrusterVars == null)
                thrusterVars = forward;
            //thrusterVars.PositiveThrusters[0].W

            Matrix localOrientation;
            thrusterVars.PositiveThrusters[0].Orientation.GetMatrix(out localOrientation);
            ///var rotationCrossVector = Vector3D.Cross();
            float angle = Math.Abs((float)Vector3D.Dot(Vector3D.Normalize(localOrientation.Forward), Vector3D.Normalize(Vector3D.Transform(settings.remote.GetNaturalGravity(), MatrixD.Transpose(settings.positionReferenceBlock.WorldMatrix.GetOrientation())))) * 50);

            //angle = Math.Atan2(angle, Math.Sqrt(Math.Max(0.0, 1.0 - angle * angle))); //More numerically stable than: ang=Math.Asin(ang)

            gp.Echo("angle from grav: " + angle);

            // Our PID variables for tuning
            float p = (0.02f * Math.Max(targetSpeed / 104.7f, 0.05f)) * angle;
            float i = (0.03f * Math.Max(targetSpeed / 104.7f, 0.05f));// * angle;
            float d = 0.06f;

            // Temp thrust variable, thrust is worked out on a percent basis.
            float thrust = 0f;


            float speedDifference = (targetSpeed - thrusterVars.Speed);

            // Add total time of error (The we are not at target speed +/- 10% MoE)
            if (speedDifference <= targetSpeed * 0.1f && speedDifference >= -targetSpeed * 0.1f ||
                (targetSpeed < 10f && speedDifference <= Math.Max(targetSpeed, 0.4f) * 0.25f && speedDifference >= -Math.Max(targetSpeed, 0.4f) * 0.25f))
                time = 0f; // Reset as we are at target speed
            else
                time += tickTime;

            // The PID formula
            float change = (p * (speedDifference) * tickTime) +
                (i * ((speedDifference > 0f) ? time : -time) * tickTime) -
                (d * (thrusterVars.Acceleration) * tickTime);

            // Debug Info
            gp.Echo("et: " + tickTime + "\nSpeedDiff: " + speedDifference + "\n change: " + change);

            // Add change to our current thrust
            thrust = thrusterVars.Thrust + change;

            // Can only be 100% to -100% (1f to -1f)
            thrust = Math.Min(thrust, 1f);
            thrust = Math.Max(thrust, -1f);
            // Set the thrust in stored var for next run
            thrusterVars.Thrust = thrust;

            // Debug Info
            gp.Echo("Thrust:" + thrust * 100 + "/" + thrust * 100);

            // if its positive thrust, set positive thrusters and clear negative thrusters
            if (thrust > 0.001f)
            {
                thrusterVars.NegativeThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });
                thrusterVars.PositiveThrusters.ForEach(th =>
                {
                    (th as IMyThrust).SetValueFloat("Override", (th as IMyThrust).GetMaxThrust() * thrust);
                });
            }
            else if (thrust < 0.001f) // Otherwise do the opposite
            {
                thrusterVars.PositiveThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });

                thrusterVars.NegativeThrusters.ForEach(th =>
                {
                    (th as IMyThrust).SetValueFloat("Override", (th as IMyThrust).GetMaxThrust() * -thrust);
                });
            }

        }
    }

    public class YaNavThrusterControlException : YaNavException
    {
        public YaNavThrusterControlException(string message)
            : base("ThrusterControl: " + message)
        {

        }
    }
    public class YaNavThrusters
    {
        public List<IMyTerminalBlock> Forward, Backward, Left, Right, Up, Down;

        public YaNavThrusters(MyGridProgram gp, YaNavThrusterGroupNames thrusterGroupNames)
        {
            Forward = gp.GetBlockGroup(thrusterGroupNames.Forward);
            Backward = gp.GetBlockGroup(thrusterGroupNames.Backward);
            Left = gp.GetBlockGroup(thrusterGroupNames.Left);
            Right = gp.GetBlockGroup(thrusterGroupNames.Right);
            Up = gp.GetBlockGroup(thrusterGroupNames.Up);
            Down = gp.GetBlockGroup(thrusterGroupNames.Down);
        }

    }

    public class YaNavThrusterVars
    {
        public List<IMyTerminalBlock> PositiveThrusters, NegativeThrusters;
        public float Speed, LastSpeed, Acceleration, Thrust;

        public void SetSpeeds(float tickSpeed, float tickTime)
        {
            Speed = tickSpeed / tickTime;
            Acceleration = (float)Math.Round((Speed - LastSpeed) / tickTime,4);
            LastSpeed = (float)Math.Round(Speed,4);
        }
    }
    public class YaNavThrusterGroupNames
    {
        public string Forward = "", Backward = "", Left = "", Right = "",
        Up = "", Down = "";

    }
    public class YaNavThrusterSettings
    {
        public IMyTerminalBlock positionReferenceBlock;
        public IMyRemoteControl remote;
        public YaNavThrusterGroupNames thrusterGroupNames;
        public int tickCount = 15;
        public bool inNatrualGravityOnly = true;
    }
}