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
    static class GyroExtensions
    {

        public static void OverideOn(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", true);
        }
        public static void OverideOff(this IMyGyro gyroscope)
        {
            gyroscope.SetValueBool("Override", false);
        }
        public static void Stop(this IMyGyro gyroscope)
        {
            gyroscope.SetPitch(0f);
            gyroscope.SetYaw(0f);
            gyroscope.SetRoll(0f);
        }
        public static bool Rotate(this IMyGyro gyroscope, Vector3D rotationVector, Vector3D? currentVector = null, float coEff = 0.8f)
        {

            if (!currentVector.HasValue)
            {
                Matrix localOrientation;
                gyroscope.Orientation.GetMatrix(out localOrientation);
                currentVector = localOrientation.Forward;
            }
            // Get the current vector for our chosen direction
            // If we are aligned to the targetVector, stop overides, turn override off and return true
            if (gyroscope.IsAligned(rotationVector, currentVector.Value))
            {
                gyroscope.Stop();
                gyroscope.OverideOff();
                return true;
            }

            // Otherwise try align to targetVector

            // Transform to rotationVector into a vector relative to the gyroscrope
            var targetVector = Vector3D.Transform(rotationVector, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));
            var rotationCrossVector = Vector3D.Cross(currentVector.Value, targetVector);
            double angle = rotationCrossVector.Length();
            angle = Math.Atan2(angle, Math.Sqrt(Math.Max(0.0, 1.0 - angle * angle))); //More numerically stable than: ang=Math.Asin(ang)
            double ctrlVelocity = gyroscope.GetMaximum<float>("Yaw") * (angle / Math.PI) * coEff;
            ctrlVelocity = Math.Min(gyroscope.GetMaximum<float>("Yaw"), ctrlVelocity);
            ctrlVelocity = Math.Max(0.01, ctrlVelocity); //Gyros don't work well at very low speeds
            rotationCrossVector.Normalize();
            rotationCrossVector *= ctrlVelocity;
            gyroscope.SetPitch((float)rotationCrossVector.GetDim(0));
            gyroscope.SetYaw((float)-rotationCrossVector.GetDim(1));
            gyroscope.SetRoll((float)-rotationCrossVector.GetDim(2));
            gyroscope.OverideOn();
            return false;
        }

       
        public static void SetPitch(this IMyGyro gyroscope, float pitch)
        {
            gyroscope.SetValueFloat("Pitch", pitch);
        }
        public static void SetYaw(this IMyGyro gyroscope, float yaw)
        {
            gyroscope.SetValueFloat("Yaw", yaw);
        }
        public static void SetRoll(this IMyGyro gyroscope, float roll)
        {
            gyroscope.SetValueFloat("Roll", roll);
        }
    }
}
