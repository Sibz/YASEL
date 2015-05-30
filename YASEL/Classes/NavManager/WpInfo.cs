using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sandbox.Common;
using Sandbox.Common.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Engine;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game;
using VRageMath;


namespace Nav.WpInfo
{
    
    public class WpInfo
    {
        public string type, direction;
        public bool enabled, traveled, aligned, leveled, reAligning, waited, centered, alignFirst, noStop;
        public double speed,
                        stopDist,
                        waitTime,
                        thrust;
        public Vector3D pos,
                        cross,
                        angle;
        public int wpId;

        public Action CallbackMethod;

        public WpInfo()
        {
            enabled = traveled = aligned = leveled = waited = centered = true;
        }


        public string GetLCDString()
        {
            return "enabled=" + enabled + ";type=" + type +
                        (speed == 0 ? "" : ";speed=" + speed) +
                        (thrust == 0 ? "" : ";thrust=" + thrust) +
                        (stopDist == 0 ? "" : ";stopDist=" + stopDist) +
                        (waitTime == 0 ? "" : ";waitTime=" + waitTime) +
                        (alignFirst == false ? "" : ";alignFirst=" + alignFirst) +
                        (noStop == false ? "" : ";noStop=" + noStop) +
                        (isZeroVector(pos) ? "" : ";pos=" + vToStr(pos)) +
                        (isZeroVector(angle) ? "" : ";angle=" + vToStr(angle)) +
                        (isZeroVector(cross) ? "" : ";cross=" + vToStr(cross));
        }

        public void InitFromString(string initString)
        {
            string[] idVars = initString.Split(new char[] { ':' });
            wpId = Convert.ToInt32(idVars[0]);
            string[] vars = idVars[1].Split(new char[] { ';' });
            Array.ForEach(vars, curVar =>
            {
                var key = curVar.Split(new char[] { '=' })[0];
                var val = curVar.Split(new char[] { '=' })[1];
                if (key == "type")
                    type = val;
                else if (key == "speed")
                    speed = strToD(val);
                else if (key == "direction")
                    direction = val;
                else if (key == "thrust")
                    thrust = strToD(val);
                else if (key == "stopDist")
                    stopDist = strToD(val);
                else if (key == "waitTime")
                    waitTime = strToD(val);
                else if (key == "alignFirst")
                    alignFirst = strToB(val);
                else if (key == "noStop")
                    noStop = strToB(val);
                else if (key == "pos")
                    pos = strToV(val);
                else if (key == "angle")
                    angle = strToV(val);
                else if (key == "cross")
                    cross = strToV(val);
            });
        }
        public void TravelTo(Nullable<Vector3D> v = null) { if (v != null) { pos = (Vector3D)v; } traveled = false; }
        public void AlignTo(Nullable<Vector3D> v = null) { if (v != null) { angle = (Vector3D)v; } aligned = false; }
        public void LevelTo(Nullable<Vector3D> v = null) { if (v != null) { cross = (Vector3D)v; } leveled = false; }
        public void CenterOn(Nullable<Vector3D> v = null) { if (v != null) { pos = (Vector3D)v; } centered = false; }
        public void WaitFor(int numSecs = 0) { if (numSecs != 0)waitTime = numSecs; waited = false; }
        public bool IsComplete() { return traveled && aligned && leveled && waited && centered; }
        public bool isZeroVector(Vector3D v) { return (v.GetDim(0) == 0 && v.GetDim(2) == 0 && v.GetDim(2) == 0); }
        private bool strToB(string b) { return (b == "true" || b == "True"); }
        private double strToD(string d) { return Convert.ToDouble(d); }
        private Vector3D strToV(string v) { string[] xyz = v.Split(new char[] { ',' }); return new Vector3D(strToD(xyz[0]), strToD(xyz[1]), strToD(xyz[2])); }
        private string vToStr(Vector3D v) { return Math.Round(v.GetDim(0), 14) + "," + Math.Round(v.GetDim(1), 14) + "," + Math.Round(v.GetDim(2), 14); }

    }
}
