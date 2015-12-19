using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavBlockExtensions
{

    using GyroExtensions;

    public static class YaNavBlockExtensions
    {

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

        public static bool Rotate(this IMyGyro gyroscope, Vector3 targetVector, Vector3? indicatorVector = null, float coEff = 0.8f, float accuracy = 0.01f)
        {

            if (!indicatorVector.HasValue) // Use Gyroscope forward vector if none given, doesn't support gyros place in different directions
                indicatorVector = gyroscope.GetDirectionalVector(); // gets the forward vector
            else
            {
                // transform indicatorVector into one for the gyroscope, supports gyros placed in different directions
                Matrix localOrientation;
                gyroscope.Orientation.GetMatrix(out localOrientation);
                Matrix.Transpose(ref localOrientation, out localOrientation);
                indicatorVector = Vector3.Transform(indicatorVector.Value, localOrientation);
            }
            
            // Transform to targetVector into a vector relative to the gyroscrope
            targetVector = Vector3.Transform(targetVector, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));

            float angle;
            Vector3 rotationAxis;
            GetChangeInDirection(indicatorVector.Value, targetVector, out rotationAxis, out angle);

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

        // Rotation methods made with help from JoeTheDestoyer
        // See Thread: http://forums.keenswh.com/threads/gravity-aware-rotation.7376549/
        public static bool Rotate(this IMyGyro gyroscope, Vector3 targetDirection, Vector3 gravDirection, Vector3 targetIndicator, Vector3 gravityIndicator, float coEff = 0.8f, float accuracy = 0.01f)
        {
            // transform indicators into ones for the gyroscope, supports gyros placed in different directions
            Matrix localOrientation;
            gyroscope.Orientation.GetMatrix(out localOrientation);
            Matrix.Transpose(ref localOrientation, out localOrientation);
            targetIndicator = Vector3.Negate(Vector3.Transform(targetIndicator, localOrientation));
            gravityIndicator = Vector3.Negate(Vector3.Transform(gravityIndicator, localOrientation));

            // Transform to targetVector into a vector relative to the gyroscrope
            targetDirection = Vector3.Transform(targetDirection, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));
            gravDirection = Vector3.Transform(gravDirection, MatrixD.Transpose(gyroscope.WorldMatrix.GetOrientation()));

            var closestGravDirectionMatch = GetClosest90DegreeVector(targetDirection, gravDirection);
            
            float angle;
            Vector3 rotationAxis;
            GetChangeInPose(targetDirection, closestGravDirectionMatch, targetIndicator, gravityIndicator, out rotationAxis, out angle);
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
        public static void GetChangeInPose(Vector3 V1, Vector3 V2, Vector3 T1, Vector3 T2, out Vector3 rotationAxis, out float angle)
        {
            Vector3 rotationAxis1, rotationAxis2;
            float RotationAngle1, RotationAngle2;

            GetChangeInDirection(V1, T1, out rotationAxis1, out RotationAngle1);
            var R1 = Quaternion.CreateFromAxisAngle(rotationAxis1, RotationAngle1);
            var V2a = Vector3.Transform(V2, R1);
            
            GetChangeInDirection(V2a, T2, out rotationAxis2, out RotationAngle2);
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

        // Thanks to JoeTheDestroyer and Naosyth for working examples to base this function on.
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

    }
}