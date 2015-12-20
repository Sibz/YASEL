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
        float currentSpeed, tickTime;
        YaNavThrusterVars forward, right, up;
        YaNavThrusterSettings settings;

        public YaNavThrusterControl(MyGridProgram gp, YaNavThrusterSettings settings)
        {
            // Set Class Variables
            this.gp = gp;
            this.settings = settings;

            // Set up thruster groups
            thrusters = new YaNavThrusters(gp, settings.Remote);
            if (thrusters.Forward.Count == 0 ||
                thrusters.Backward.Count == 0 ||
                thrusters.Left.Count == 0 ||
                thrusters.Right.Count == 0 ||
                thrusters.Up.Count == 0)
                throw new YaNavThrusterControlException(lang.ErrorNoThrusterBlocks);

            if (thrusters.Down.Count == 0 && !settings.InNatrualGravityOnly)
                throw new YaNavThrusterControlException(lang.ErrorNoDownThrusters);

            if (settings.Debug.Contains("initThruster")) 
                gp.Echo("ThrusterControl: tickCount:" + settings.TickCount);
            // Calculate the game time per run
            tickTime = 1f * ((float)settings.TickCount / 60f);

            // Variable stores for each thrust axis
            forward = new YaNavThrusterVars(thrusters.Forward, thrusters.Backward, tickTime);
            right = new YaNavThrusterVars(thrusters.Right, thrusters.Left, tickTime);
            up = new YaNavThrusterVars(thrusters.Up, thrusters.Down, tickTime);

            // Set last position
            lastPosition = settings.Remote.GetPosition();



        }
        public void Tick()
        {
            // Get the change in position
            Vector3 changeInPosition = settings.Remote.GetPosition() - lastPosition;

            // reset last position
            lastPosition = settings.Remote.GetPosition();

            // Calculate overall speed            
            currentSpeed = changeInPosition.Length() / tickTime;

            // Calculate speeds(and acceleration for each axis)
            forward.SetSpeeds(Vector3.Dot(changeInPosition, settings.Remote.WorldMatrix.Forward));
            right.SetSpeeds(Vector3.Dot(changeInPosition, settings.Remote.WorldMatrix.Right));
            up.SetSpeeds(Vector3.Dot(changeInPosition, settings.Remote.WorldMatrix.Up));

            if (settings.Debug.Contains("tickThruster")) gp.Echo("Speed:" + currentSpeed + " m/s" + "\n" +
            "\n Forward: " + forward.Speed +
            "\n Right: " + right.Speed +
                "\n Up: " + up.Speed +
                "\n AccelForward:" + forward.Acceleration +
                "\n AccelRight:" + right.Acceleration +
                "\n AccelUp:" + up.Acceleration);
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
        public void MoveAngle(Vector3D angle, float targetSpeed = 50)
        {
            var angularVelocity = (targetSpeed / (angle.Length() * 100) * 100);
            if (settings.Debug.Contains("moveAngle")) gp.Echo("moveAngle angularVelocity:" + angularVelocity);
            MoveRight((float)(angularVelocity * angle.GetDim(0)));
            MoveUp((float)(angularVelocity * angle.GetDim(1)));
            MoveForward((float)(angularVelocity * -angle.GetDim(2)));
        }
        public void StopThrusters()
        {
            thrusters.Forward.ForEach(t => { t.SetValueFloat("Override", 0f); });
            thrusters.Backward.ForEach(t => { t.SetValueFloat("Override", 0f); });
            thrusters.Left.ForEach(t => { t.SetValueFloat("Override", 0f); });
            thrusters.Right.ForEach(t => { t.SetValueFloat("Override", 0f); });
            thrusters.Up.ForEach(t => { t.SetValueFloat("Override", 0f); });
            thrusters.Down.ForEach(t => { t.SetValueFloat("Override", 0f); });
        }

        private void move(float targetSpeed = 50, YaNavThrusterVars thrusterVars = null)
        {
            if (thrusterVars == null)
                thrusterVars = forward;

            // Get orientation matrix for formula below
            Matrix localOrientation;
            thrusterVars.PositiveThrusters[0].Orientation.GetMatrix(out localOrientation);

            // Magic formula to create a gravity coefficient.
            float gravCoEff = Math.Max(1f, Math.Abs((float)Vector3D.Dot(Vector3D.Normalize(localOrientation.Forward), Vector3D.Normalize(Vector3D.Transform(settings.Remote.GetNaturalGravity(), MatrixD.Transpose(settings.Remote.WorldMatrix.GetOrientation())))) * 5));
            // Forumla for speed coefficient
            float speedCoEff = Math.Max(targetSpeed / settings.MaxSpeed, 0.3f);

            if (settings.Debug.Contains("move")) gp.Echo("gravity coefficient: " + gravCoEff);

            // Our PID variables for tuning
            float p = 0.10f * settings.MassCoEff * speedCoEff * gravCoEff;
            float i = 0.15f * settings.MassCoEff * speedCoEff;
            float d = 0.24f * settings.MassCoEff;

            // Temp thrust variable, thrust is worked out on a percent basis.
            float thrust = 0f;

            float speedDifference = (targetSpeed - thrusterVars.Speed);

            // Add total time of error (That we are not at target speed +/- 0.2m/s or +/- 10% for speeds higher than 10m/s )
            if ((speedDifference <= 0.2f && speedDifference >= -0.2f) || (speedDifference <= targetSpeed * 0.1f && speedDifference >= -targetSpeed * 0.1f))
                thrusterVars.Time = 0f; // Reset as we are at target speed
            else
                thrusterVars.Time += tickTime;

            // The PID formula
            float change = (p * (speedDifference) * tickTime) +
                (i * ((speedDifference > 0f) ? thrusterVars.Time : -thrusterVars.Time) * tickTime) -
                (d * (thrusterVars.Acceleration) * tickTime);

            if (settings.Debug.Contains("move")) gp.Echo("tickTime: " + tickTime + "\nSpeedDiff: " + speedDifference + "\n change: " + change);

            // Add change to our current thrust
            thrust = thrusterVars.Thrust + change;

            // Can only be 100% to -100% (1f to -1f)
            thrust = Math.Min(thrust, 1f);
            thrust = Math.Max(thrust, -1f);

            // Set the thrust in stored var for next run
            thrusterVars.Thrust = thrust;

            if (settings.Debug.Contains("move")) gp.Echo("Thrust:" + thrust * 100 + "/" + thrust * 100);

            // if its positive thrust, set positive thrusters and clear negative thrusters
            if (thrust > settings.SettleThrustPercent)
            {
                thrusterVars.NegativeThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });
                thrusterVars.PositiveThrusters.ForEach(th =>
                {
                    (th as IMyThrust).SetValueFloat("Override", (th as IMyThrust).GetMaxThrust() * Math.Max(thrust, settings.MinThrustPercent));
                });
            }
            else if (thrust < -settings.SettleThrustPercent) // Otherwise do the opposite
            {
                thrusterVars.PositiveThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });
                thrusterVars.NegativeThrusters.ForEach(th =>
                {
                    (th as IMyThrust).SetValueFloat("Override", (th as IMyThrust).GetMaxThrust() * Math.Max(-thrust, settings.MinThrustPercent));
                });
            }
            else // if thrust is tiny value turn off overide
            {
                thrusterVars.PositiveThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });
                thrusterVars.NegativeThrusters.ForEach(th => { (th as IMyThrust).SetValueFloat("Override", 0f); });
                thrusterVars.Time = 0f; // Avoid big fluctuations while settling
            }
        }
    }

    public class YaNavThrusterControlException : YaNavException { public YaNavThrusterControlException(string message) : base("ThrusterControl: " + message) { } }

    public class YaNavThrusters
    {
        public List<IMyTerminalBlock> Forward, Backward, Left, Right, Up, Down;

        public YaNavThrusters(MyGridProgram gp, IMyTerminalBlock Remote)
        {
            if (Remote == null)
                throw new YaNavThrusterControlException(lang.ErrorNoRemote);
            Forward = new List<IMyTerminalBlock>();
            Backward = new List<IMyTerminalBlock>();
            Left = new List<IMyTerminalBlock>();
            Right = new List<IMyTerminalBlock>();
            Up = new List<IMyTerminalBlock>();
            Down = new List<IMyTerminalBlock>();

            var allThrusters = new List<IMyTerminalBlock>();
            gp.GridTerminalSystem.GetBlocksOfType<IMyThrust>(allThrusters, b => { return b.CubeGrid == gp.Me.CubeGrid; });

            Matrix fromGridToReference;
            Remote.Orientation.GetMatrix(out fromGridToReference);
            Matrix.Transpose(ref fromGridToReference, out fromGridToReference);
            Matrix identity = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
            allThrusters.ForEach(thruster =>
            {
                Matrix fromThrusterToGrid;
                thruster.Orientation.GetMatrix(out fromThrusterToGrid);

                Vector3D accelerationDirection = Vector3.Transform(fromThrusterToGrid.Backward, fromGridToReference);

                if (accelerationDirection == identity.Forward)
                    Forward.Add(thruster);
                else if (accelerationDirection == identity.Backward)
                    Backward.Add(thruster);
                else if (accelerationDirection == identity.Left)
                    Left.Add(thruster);
                else if (accelerationDirection == identity.Right)
                    Right.Add(thruster);
                else if (accelerationDirection == identity.Up)
                    Up.Add(thruster);
                else if (accelerationDirection == identity.Down)
                    Down.Add(thruster);
            });
        }
    }

    public class YaNavThrusterVars
    {
        public List<IMyTerminalBlock> PositiveThrusters, NegativeThrusters;
        public float Speed, LastSpeed, Acceleration, Thrust, Time, TickTime;
        public YaNavThrusterVars(List<IMyTerminalBlock> positiveThrusters, List<IMyTerminalBlock> negativeThrusters, float tickTime)
        {
            PositiveThrusters = positiveThrusters;
            NegativeThrusters = negativeThrusters;
            TickTime = tickTime;
        }

        public void SetSpeeds(float tickSpeed)
        {
            Speed = tickSpeed / TickTime;
            Acceleration = (float)Math.Round((Speed - LastSpeed) / TickTime, 4);
            LastSpeed = (float)Math.Round(Speed, 4);
        }
    }
    public class YaNavThrusterSettings
    {
        public IMyRemoteControl Remote;
        public int TickCount = 15;
        public bool InNatrualGravityOnly = true;
        public float MassCoEff = 1f;
        public float MaxSpeed = 100f;
        public float MinThrustPercent = 0.011f;
        public float SettleThrustPercent = 0.005f;
        public List<string> Debug;
        public YaNavThrusterSettings()
        {
            Debug = new List<string>();
        }
    }
}