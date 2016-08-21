using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace GyroExtensions
{
    using BlockExtensions;

    /// <summary>
    /// Common Gyroscope functions
    /// </summary>
    /// 
    public static class GyroExtensions
    {
        /// <summary>
        /// Retreives a local or grid vector from a block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="direction">Direction of vector you wish to retreive, default forward</param>
        /// <param name="grid">Whether returned vector is relative to block (false) or local grid (true)</param>
        /// <returns>Local or grid vector from a block</returns>
        public static Vector3D GetDirectionalVector(this IMyTerminalBlock block, string direction = "forward", bool grid = false)
        {
            Matrix localOrientation;
            block.Orientation.GetMatrix(out localOrientation);
            if (grid)
                Matrix.Transpose(ref localOrientation, out localOrientation);
            Vector3D refVec =
                direction == "left" ? localOrientation.Left :
                direction == "right" ? localOrientation.Right :
                direction == "up" ? localOrientation.Up :
                direction == "down" ? localOrientation.Down :
                direction == "backward" ? localOrientation.Backward :
                localOrientation.Forward;
            return refVec;
        }



        /// <summary>
        /// Rotate ship around a single axis to align indicator to target
        /// </summary>
        /// <param name="gyroscope"></param>
        /// <param name="target">
        /// Vector that points towards your target
        /// </param>
        /// <param name="indicator">
        /// Vector that you want to align to target, i.e. forward vector
        ///  if null, uses gyroscopes forward vector - will not work if 
        /// calling on multiple gyroscopes facing different directions
        ///  To use multiple gyroscopes facing in different directions use
        /// a vector from any block, I.e. a remotes forward vector. The
        /// GetDirectionalVector function can be used to retrieve this.
        /// </param>
        /// <param name="coEff"></param>
        /// <param name="accuracy"></param>
        /// <returns>true if facing target, false if still rotating</returns>
        /// <example>
        /// var gravityDirection = remote.GetNatrualGravity();
        /// var downDirection = remote.GetDirectionVector("down", true);
        /// gyroscopes.foreach(g => 
        ///     { 
        ///         (g as IMyGyro).Rotate(gravityDirection, downDirection); 
        ///     });
        /// </example>
        /// 

        public static bool Rotate(this IMyGyro gyroscope, Vector3 target, Vector3? indicator = null, float coEff = 0.8f, float accuracy = 0.01f)
        {

            if (!indicator.HasValue) // Use Gyroscope forward vector if none given, doesn't support gyros place in different directions
                indicator = gyroscope.GetDirectionalVector(); // gets the forward vector
            else
            {
                // transform indicatorVector into one for the gyroscope, supports gyros placed in different directions
                Matrix localOrientation;
                gyroscope.Orientation.GetMatrix(out localOrientation);
                Matrix.Transpose(ref localOrientation, out localOrientation);
                indicator = Vector3.Transform(indicator.Value, localOrientation);
            }

            // Transform to targetVector into a vector relative to the gyroscrope
            target = Vector3.Transform(target, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));

            float angle;
            Vector3 rotationAxis;
            GetChangeInDirection(indicator.Value, target, out rotationAxis, out angle);

            // If we are aligned to the targetVector, stop overides, turn override off and return true
            if (angle < accuracy)
            {
                gyroscope.Stop();
                gyroscope.OverrideOff();
                return true;
            }
            // not aligned so rotate
            gyroscope.Rotate(rotationAxis, (float)angle, coEff);
            return false;
        }

        public static bool Rotate(this List<IMyTerminalBlock> gyros, Vector3 target, Vector3? indicator = null, float coEff = 0.8f, float accuracy = 0.01f)
        {
            bool result = true;
            foreach (IMyTerminalBlock gyro in gyros)
            {
                if (gyro is IMyGyro)
                    if (!(gyro as IMyGyro).Rotate(target, indicator, coEff, accuracy))
                        result = false;
            }
            return result;
        }

        /// Rotation methods made with help from JoeTheDestoyer
        /// See Thread: forums.keenswh.com/threads/gravity-aware-rotation.7376549
        /// <summary>
        /// Rotate ship usin two target vectors and two indicator vectors
        /// Enables rotation around two axis thus enabling the rotation to not flip
        /// the ship upside down.
        /// </summary>
        /// <param name="gyroscope"></param>
        /// <param name="targetIndicator">Vector that you want to align to targetDirection</param>
        /// <param name="gravityIndicator">Vector (usually down) that you want to align to to gravDirection (as close as possible)</param>
        /// <param name="targetDirection">Vector that points towards your target</param>
        /// <param name="gravDirection">Direction of gravity (use GetNatrualGravity() on remote)</param>
        /// <param name="coEff"></param>
        /// <param name="accuracy"></param>
        /// <returns>true if facing targets, false if still rotating</returns>
        /// 
        public static bool Rotate(this IMyGyro gyroscope, Vector3 targetIndicator, Vector3 gravityIndicator, Vector3 targetDirection, Vector3 gravDirection, float coEff = 0.8f, float accuracy = 0.01f)
        {
            // transform indicators into ones for the gyroscope, supports gyros placed in different directions
            Matrix localOrientation;
            gyroscope.Orientation.GetMatrix(out localOrientation);
            Matrix.Transpose(ref localOrientation, out localOrientation);
            targetIndicator = Vector3.Transform(targetIndicator, localOrientation);
            gravityIndicator = Vector3.Transform(gravityIndicator, localOrientation);

            // Transform to targetVector into a vector relative to the gyroscrope
            targetDirection = Vector3.Transform(targetDirection, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));
            gravDirection = Vector3.Transform(gravDirection, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));

            var closestGravDirectionMatch = GetClosest90DegreeVector(targetDirection, gravDirection);

            float angle;
            Vector3 rotationAxis;
            GetChangeInPose(targetIndicator, gravityIndicator, targetDirection, closestGravDirectionMatch, out rotationAxis, out angle);
            if (angle < accuracy)
            {
                gyroscope.Stop();
                gyroscope.OverrideOff();
                return true;
            }
            // not aligned so rotate
            gyroscope.Rotate(rotationAxis, angle, coEff);

            return false;
        }

        // Thanks to JoeTheDestroyer for this function and explaination
        public static void GetChangeInDirection(Vector3 indicator, Vector3 target, out Vector3 rotationAxis, out float angle)
        {
            rotationAxis = Vector3.Cross(indicator, target);
            angle = rotationAxis.Normalize();
            angle = (float)Math.Atan2((double)angle, Math.Sqrt(Math.Max(0.0, 1.0 - (double)angle * (double)angle)));
        }
        // Thanks to JoeTheDestroyer for this function and explaination
        public static void GetChangeInPose(Vector3 indicator1, Vector3 indicator2, Vector3 target1, Vector3 target2, out Vector3 rotationAxis, out float angle)
        {
            Vector3 rotationAxis1, rotationAxis2;
            float RotationAngle1, RotationAngle2;

            GetChangeInDirection(indicator1, target1, out rotationAxis1, out RotationAngle1);
            var R1 = Quaternion.CreateFromAxisAngle(rotationAxis1, RotationAngle1);
            var V2a = Vector3.Transform(indicator2, R1);

            GetChangeInDirection(V2a, target2, out rotationAxis2, out RotationAngle2);
            var R2 = Quaternion.CreateFromAxisAngle(rotationAxis2, RotationAngle2);
            var R3 = R2 * R1;
            R3.GetAxisAngle(out rotationAxis, out angle);
        }
        // Thanks to JoeTheDestroyer for this function and explaination
        public static Vector3 GetClosest90DegreeVector(Vector3 mainTarget, Vector3 secondaryTarget)
        {
            var perp = Vector3.Cross(mainTarget, secondaryTarget);
            perp.Normalize();
            return Vector3.Cross(perp, mainTarget);
        }

        /// Thanks to JoeTheDestroyer and Naosyth for working examples to base this function on.
        /// See Thread: forums.keenswh.com/threads/aligning-ship-to-planet-gravity.7373513/

        public static void Rotate(this IMyGyro gyroscope, Vector3D rotationAxis, float angle, float coEff = 0.8f)
        {
            float ctrlVelocity = gyroscope.GetMaximum<float>("Yaw") * (float)(angle / Math.PI) * coEff;
            ctrlVelocity = Math.Min(gyroscope.GetMaximum<float>("Yaw"), ctrlVelocity);
            ctrlVelocity = (float)Math.Max(0.01, ctrlVelocity);
            rotationAxis.Normalize();
            rotationAxis *= ctrlVelocity;
            gyroscope.SetPitch((float)rotationAxis.GetDim(0));
            gyroscope.SetYaw(-(float)rotationAxis.GetDim(1));
            gyroscope.SetRoll(-(float)rotationAxis.GetDim(2));
            gyroscope.OverrideOn();
        }
        public static void OverrideOn(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", true);
        }
        public static void OverrideOff(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", false);
        }
        public static void OverrideOff(this List<IMyTerminalBlock> gyros)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro)
                    (gyro as IMyGyro).OverrideOff();
        }
        public static void OverrideOn(this List<IMyTerminalBlock> gyros)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro)
                    (gyro as IMyGyro).OverrideOn();
        }
        public static void Stop(this IMyGyro gyroscope)
        {
            gyroscope.SetPitch(0f);
            gyroscope.SetYaw(0f);
            gyroscope.SetRoll(0f);
        }
        public static void Stop(this List<IMyTerminalBlock> gyros)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro)
                    (gyro as IMyGyro).Stop();
        }
        public static void SetPitch(this IMyGyro gyroscope, float pitch)
        {
            gyroscope.SetValueFloat("Pitch", pitch);
        }
        public static void SetPitch(this List<IMyTerminalBlock> gyros, float pitch)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro)
                    (gyro as IMyGyro).SetPitch(pitch);
        }
        public static bool IsPitching(this IMyGyro gyroscope, float accuracy = 0.001f)
        {
            return gyroscope.GetValueFloat("Pitch") > accuracy || gyroscope.GetValueFloat("Pitch") < -accuracy;
        }
        public static bool IsPitching(this List<IMyTerminalBlock> gyros, float accuracy = 0.001f)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro && (gyro as IMyGyro).IsPitching(accuracy)) return true;
            return false;
        }
        public static void SetYaw(this IMyGyro gyroscope, float yaw)
        {
            gyroscope.SetValueFloat("Yaw", yaw);
        }
        public static void SetYaw(this List<IMyTerminalBlock> gyros, float yaw)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro)
                    (gyro as IMyGyro).SetPitch(yaw);
        }
        public static void SetRoll(this IMyGyro gyroscope, float roll)
        {
            gyroscope.SetValueFloat("Roll", roll);
        }
        public static bool IsRolling(this IMyGyro gyroscope, float accuracy = 0.001f)
        {
            return gyroscope.GetValueFloat("Roll") > accuracy || gyroscope.GetValueFloat("Roll") < -accuracy;
        }
        public static bool IsRolling(this List<IMyTerminalBlock> gyros, float accuracy = 0.001f)
        {
            foreach (var gyro in gyros)
                if (gyro is IMyGyro && (gyro as IMyGyro).IsRolling(accuracy)) return true;
            return false;
        }

    }
}
