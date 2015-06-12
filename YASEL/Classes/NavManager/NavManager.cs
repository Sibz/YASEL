using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;


namespace Nav.NavManager
{
    using Nav.WpInfo;
    using VarStore;
    using Nav.NavSettings;
    using Gyro;
    using Thruster;
    using Grid;
    using Cockpit;

    public class NavManager
    {
        public const string ERROR1 = "Nav Error 1: Unable to initialise nav, unable to acquire reference to one or more of position blocks. Check the names and that they exist.";
        public const string ERROR2 = "Nav Error 2: Unable to initialise nav, unable to acquire list of gyros check group name, and that there is atleast one gyro.";
        public const string ERROR3 = "Nav Error 3: Unable to initialise nav, unable to acquire list of fwd or rvs thrusters check group name, and that there is atleast one thruster.";
        public const string ERROR4 = "Nav Error 4: Unable to initialise nav, unable to acquire reference to a cockpit or remote control";

        private VarStore vs;

        private NavSettings settings;

        private WpInfo wpInfo;

        private Dictionary<int, WpInfo> waypoints;

        Vector3D lPos, rPos, bPos, tPos, rearPos, fwdPos;

        double speed, avgSpeed, lastDistToDest;
        double[] speeds, speedsToDest;

        int runCount;

        bool paused;

        IMyTerminalBlock leftBlock,
                                rightBlock,
                                topBlock,
                                bottomBlock,
                                rearBlock,
                                fwdBlock,
                                controller;

        IMyTextPanel lcdWaypoints;

        List<IMyTerminalBlock> gyros,
                                fwdThrusters,
                                rvsThrusters,
                                leftThrusters,
                                rightThrusters,
                                upThrusters,
                                downThrusters;

        Nullable<DateTime> waitTime;

        public NavManager(NavSettings s = null)
        {
            settings = (s == null) ? new NavSettings() : s;
            vs = new VarStore(s.LCDVariableStoreName);
            controller = null;


            //LCD.dbug("Initialising NavManager", 2);


            runCount = 0;
            // Setup GPS Blocks
            leftBlock = Grid.GetBlock(settings.LeftBlockName);
            rightBlock = Grid.GetBlock(settings.RightBlockName);
            topBlock = Grid.GetBlock(settings.TopBlockName);

            bottomBlock = Grid.GetBlock(settings.BottomBlockName);
            rearBlock = Grid.GetBlock(settings.RearBlockName);
            fwdBlock = Grid.GetBlock(settings.FwdBlockName);

            if (leftBlock == null || rightBlock == null || topBlock == null || bottomBlock == null || rearBlock == null || fwdBlock == null) { /*LCD.dbug(ERROR1);*/ throw new Exception(ERROR1); }
            //Setup block groups
            gyros = Grid.GetBlockGrp(settings.GyroGroupName); if (gyros.Count == 0) { /*LCD.dbug(ERROR2);*/ throw new Exception(ERROR2); }
            fwdThrusters = Grid.GetBlockGrp(settings.FwdThrustGroupName); if (fwdThrusters.Count == 0) { /*LCD.dbug(ERROR3*/ throw new Exception(ERROR3); }
            rvsThrusters = Grid.GetBlockGrp(settings.RvsThrustGroupName); if (rvsThrusters.Count == 0) { /*LCD.dbug(ERROR3);*/ throw new Exception(ERROR3); }
            leftThrusters = Grid.GetBlockGrp(settings.LeftThrustGroupName); if (leftThrusters.Count == 0) { /*LCD.dbug(ERROR3);*/ throw new Exception(ERROR3); }
            rightThrusters = Grid.GetBlockGrp(settings.RightThrustGroupName); if (rightThrusters.Count == 0) { /*LCD.dbug(ERROR3);*/ throw new Exception(ERROR3); }
            upThrusters = Grid.GetBlockGrp(settings.UpThrustGroupName); if (upThrusters.Count == 0) { /*LCD.dbug(ERROR3);*/ throw new Exception(ERROR3); }
            downThrusters = Grid.GetBlockGrp(settings.DownThrustGroupName); if (downThrusters.Count == 0) { /*LCD.dbug(ERROR3);*/ throw new Exception(ERROR3); }

            // Setup Controller Block (for dampener control)
            if (settings.CockpitName == null)
            {
                var controllers = new List<IMyTerminalBlock>();
                Grid.ts.GetBlocksOfType<IMyShipController>(controllers);
                if (controllers.Count > 0)
                    controller = controllers[0];

            }
            else
            {
                controller = Grid.GetBlock(settings.CockpitName);
            }
            if (controller == null) { /*LCD.dbug(ERROR4);*/
                throw new Exception(ERROR4); }

            waypoints = new Dictionary<int, WpInfo>();
        }

        public void Tick()
        {
            if (paused) return;
            //LCD.dbug("Tick", 2);
            lPos = leftBlock.GetPosition();
            rPos = rightBlock.GetPosition();
            tPos = topBlock.GetPosition();
            bPos = bottomBlock.GetPosition();
            rearPos = rearBlock.GetPosition();
            fwdPos = fwdBlock.GetPosition();

            if (wpInfo != null && (!wpInfo.traveled || !wpInfo.centered))
            {
                speed = (lastDistToDest == null) ? 0 : (lastDistToDest - ((wpInfo.pos - GetPos()).Length())) * (settings.TicksPerSec / settings.ActionTick);
                lastDistToDest = (wpInfo.pos - GetPos()).Length();

                speed = (speed > 1000) ? 0 : speed;

                if (speeds == null)
                {
                    speeds = new double[9];
                    speeds[0] = speed;
                }

                int i = 0;

                double[] newSpeeds = new double[9];

                for (i = 0; i < 8; i++)
                {

                    avgSpeed += speeds[i];
                    newSpeeds[i + 1] = speeds[i];
                }
                newSpeeds[0] = speed;
                avgSpeed += speed;
                avgSpeed = avgSpeed / 10;
                if (avgSpeed > 500)
                    throw new Exception("avgSpeed out of limits:" + avgSpeed.ToString());

                speeds = newSpeeds;
            }

            // In order not to slow things down, we only write stuff to LCD every 8 ticks (~2 sec)
            if (runCount > 8)
                runCount = 1;
            else if (runCount == 8)
            {
                vs.SetVarToStore("Speed", Math.Round(speed, 2).ToString());
                vs.SetVarToStore("avgSpeed", Math.Round(avgSpeed, 2).ToString());
                if (wpInfo != null)
                {
                    vs.SetVarToStore("Aligned", wpInfo.aligned.ToString());
                    vs.SetVarToStore("Leveled", wpInfo.leveled.ToString());
                    vs.SetVarToStore("Traveled", wpInfo.traveled.ToString());
                    vs.SetVarToStore("Centered", wpInfo.centered.ToString());
                }
            }
            else
                runCount++;
            //LCD.dbug("Tick - Done routine stuff, doing WP Stuff", 2);
            // If WPInfo is null try get one from the q
            if (wpInfo == null && waypoints.Count > 0)
            {
                wpInfo = getFirst();
            }
            // IF our WpInfo isn't null and we have not completed its contents, do Wp
            if (wpInfo != null && !wpInfo.IsComplete())
            {

                if (!wpInfo.traveled)
                {
                    wpInfo.traveled = travel();
                    return;
                }
                if (!wpInfo.aligned)
                {
                    wpInfo.aligned = align();
                    return;
                }
                if (!wpInfo.leveled)
                {
                    wpInfo.leveled = level();
                    return;
                }
                if (!wpInfo.waited)
                {
                    wpInfo.waited = wait();
                    return;
                }
                if (!wpInfo.centered)
                {
                    wpInfo.centered = center();
                    return;
                }
            } if (wpInfo != null && wpInfo.IsComplete())
            {
                waypoints.Remove(wpInfo.wpId);
                Action a = wpInfo.CallbackMethod;
                wpInfo = null;
                if (a != null)
                    a();
            }
            //LCD.dbug("Tick END", 2);
        }

        public void Act(WpInfo wi)
        {
            if (wi.type.Contains("wp") || wi.type.Contains("travel"))
                wi.TravelTo();
            if (wi.type.Contains("align"))
                wi.AlignTo();
            if (wi.type.Contains("level"))
                wi.LevelTo();
            if (wi.type.Contains("wait"))
                wi.WaitFor();
            if (wi.type.Contains("center"))
                wi.CenterOn();

            wi.wpId = waypoints.Count;
            if (!waypoints.ContainsKey(waypoints.Count))
                waypoints.Add(waypoints.Count, wi);
            //else
                //LCD.dbug("Unable to add waypoint - key exists\n\t" + wi.GetLCDString());
        }
        public void Act(Dictionary<int, WpInfo> wpi)
        {
            var wpiEnum = wpi.GetEnumerator();
            while (wpiEnum.MoveNext())
            {
                Act(wpiEnum.Current.Value);
            }
        }
        private WpInfo getFirst()
        {
            WpInfo wi = new WpInfo();
            wi.wpId = 99999;
            var wpEnum = waypoints.GetEnumerator();
            while (wpEnum.MoveNext())
            {
                if (wi.wpId > wpEnum.Current.Value.wpId)
                    wi = wpEnum.Current.Value;
            }
            return wi;
        }

        public bool IsActing() { return (waypoints.Count > 0); }

        public void Pause()
        {
            Gyro.YawStop(gyros);
            Gyro.PitchStop(gyros);
            Gyro.GyroOverride(gyros, false);
            Cockpit.TurnOnOffDampeners(controller, true);
            Thruster.SetThrustOverride(fwdThrusters, 0);
            Thruster.SetThrustOverride(rvsThrusters, 0);
            Thruster.SetThrustOverride(leftThrusters, 0);
            Thruster.SetThrustOverride(rightThrusters, 0);
            Thruster.SetThrustOverride(upThrusters, 0);
            Thruster.SetThrustOverride(downThrusters, 0);
            paused = true;

        }
        public void UnPause()
        {
            paused = false;
        }
        public void Reset()
        {
            Pause();
            wpInfo = null;
            waypoints = new Dictionary<int, WpInfo>();
            paused = false;
        }

        private bool wait()
        {
            if (waitTime == null)
                waitTime = DateTime.Now.AddSeconds(wpInfo.waitTime);
            if (waitTime > DateTime.Now)
                return false;
            waitTime = null;
            return true;
        }
        private bool center(WpInfo wi = null)
        {
            bool centered = false;
            wi = wi == null ? wpInfo : wi;
            Vector3D targetVector = wi.pos;
            double thrust = wi.thrust / 20;
            thrust = thrust < 2.5 ? 2.5 : thrust;
            double left = (wi.pos - GetOffset(20, "left")).Length();
            double right = (wi.pos - GetOffset(20, "right")).Length();
            double up = (wi.pos - GetOffset(20, "up")).Length();
            double down = (wi.pos - GetOffset(20, "down")).Length();
            double forward = (wi.pos - GetOffset(20, "forward")).Length();
            double backward = (wi.pos - GetOffset(20, "backward")).Length();
            double lThrust, rThrust, uThrust, dThrust, fwdThrust, rvsThrust;
            lThrust = rThrust = uThrust = dThrust = fwdThrust = rvsThrust = 0;

            if (!(left < 20.1 && left > 19.9) && left < right)
                lThrust = thrust;
            else if (!(left < 20.1 && left > 19.9))
                rThrust = thrust;
            if (!(up < 20.1 && up > 19.9) && up < down)
                uThrust = thrust;
            else if (!(up < 20.1 && up > 19.9))
                dThrust = thrust;
            if (!(forward < 20.1 && forward > 19.9) && forward < backward)
                fwdThrust = thrust;
            else if (!(forward < 20.1 && forward > 19.9))
                rvsThrust = thrust;

            if (left < 20.1 && left > 19.9 && up < 20.1 && up > 19.9 && forward < 20.1 && forward > 19.9)
                centered = true;

            if (speed > 0.1 || speed < -0.1 || centered)
                lThrust = rThrust = uThrust = dThrust = fwdThrust = rvsThrust = 0;

            Thruster.SetThrustOverride(leftThrusters, lThrust);
            Thruster.SetThrustOverride(rightThrusters, rThrust);
            Thruster.SetThrustOverride(upThrusters, uThrust);
            Thruster.SetThrustOverride(downThrusters, dThrust);
            Thruster.SetThrustOverride(fwdThrusters, fwdThrust);
            Thruster.SetThrustOverride(rvsThrusters, rvsThrust);

            return centered;
        }
        private bool level(WpInfo wi = null)
        {
            wi = wi == null ? wpInfo : wi;
            Vector3D crossVector = wi.cross;
            Gyro.GyroOverride(gyros, true);
            Vector3D shipR = rPos - lPos;
            Vector3D shipU = tPos - bPos;
            shipU.Normalize();

            double dotC = DotProduct(crossVector, shipU);
            int rollSpeed = (int)Math.Round(10 + Math.Log(dotC < 0 ? -dotC : dotC, 2));
            rollSpeed = rollSpeed <= 4 ? 4 : (rollSpeed >= 8 ? 7 : rollSpeed);
            if (dotC > settings.AlignMargin)
                Gyro.RollL(gyros, rollSpeed);
            else if (dotC < -settings.AlignMargin)
                Gyro.RollR(gyros, rollSpeed);
            else if (dotC != 0)
            {
                Gyro.RollStop(gyros);
                Gyro.GyroOverride(gyros, false);
                return true;
            }
            return false;
        }

        private bool align(WpInfo wi = null, bool dampening = true)
        {
            wi = wi == null ? wpInfo : wi;
            Vector3D targetAngle = wi.angle;
            double dotR = 0, dotU = 0;

            if (wi.direction == "left")
            {
                if (!level(new WpInfo() { cross = targetAngle })) return false;
                dotR = DotProduct(Vector3D.Normalize(Vector3D.Cross(targetAngle, tPos - bPos)), Vector3D.Normalize(rPos - lPos));
            }
            else if (wi.direction == "right")
            {
                if (!level(new WpInfo() { cross = Vector3D.Negate(targetAngle) })) return false;
                dotR = -DotProduct(Vector3D.Normalize(Vector3D.Cross(targetAngle, tPos - bPos)), Vector3D.Normalize(rPos - lPos));
            }
            else if (wi.direction == "up")
            {
                if (!level(new WpInfo() { cross = Vector3D.Cross(targetAngle, fwdPos - rearPos) })) return false;
                dotU = DotProduct(Vector3D.Normalize(Vector3D.Cross(targetAngle, rPos - lPos)), Vector3D.Normalize(tPos - bPos));
            }
            else if (wi.direction == "down")
            {
                if (!level(new WpInfo() { cross = Vector3D.Negate(Vector3D.Cross(targetAngle, fwdPos - rearPos)) })) return false;
                dotU = -DotProduct(Vector3D.Normalize(Vector3D.Cross(targetAngle, rPos - lPos)), Vector3D.Normalize(tPos - bPos));
            }
            else if (wi.direction == "reverse")
            {
                dotR = -DotProduct(targetAngle, Vector3D.Normalize(rPos - lPos));
                dotU = -DotProduct(targetAngle, Vector3D.Normalize(tPos - bPos));
            }
            else
            {
                dotR = DotProduct(targetAngle, Vector3D.Normalize(rPos - lPos));
                dotU = DotProduct(targetAngle, Vector3D.Normalize(tPos - bPos));
            }

            Gyro.GyroOverride(gyros);
            if (dampening) Cockpit.TurnOnOffDampeners(controller, true);

            int yawSpeed = (int)Math.Round(10 + Math.Log(dotR < 0 ? -dotR : dotR, 2)), pitchSpeed = (int)Math.Round(10 + Math.Log(dotU < 0 ? -dotU : dotU, 2));
            yawSpeed = yawSpeed <= 4 ? 4 : yawSpeed;
            pitchSpeed = pitchSpeed <= 4 ? 4 : pitchSpeed;
            yawSpeed = yawSpeed >= 8 ? 7 : yawSpeed;
            pitchSpeed = pitchSpeed >= 8 ? 7 : pitchSpeed;

            if (dotR > settings.AlignMargin)
                Gyro.YawR(gyros, yawSpeed);
            if (dotR < 0 - settings.AlignMargin)
                Gyro.YawL(gyros, yawSpeed);
            if (dotR < settings.AlignMargin && dotR > 0 - settings.AlignMargin)
                Gyro.YawStop(gyros);
            if (dotU > settings.AlignMargin)
                Gyro.PitchD(gyros, pitchSpeed);
            if (dotU < 0 - settings.AlignMargin)
                Gyro.PitchU(gyros, pitchSpeed);
            if (dotU < settings.AlignMargin && dotU > 0 - settings.AlignMargin)
                Gyro.PitchStop(gyros);

            if (dotR < settings.AlignMargin && dotR > 0 - settings.AlignMargin && dotU < settings.AlignMargin && dotU > 0 - settings.AlignMargin)
            { Gyro.YawStop(gyros); Gyro.PitchStop(gyros); Gyro.GyroOverride(gyros, false); return true; }

            return false;
        }

        private bool travel(WpInfo wi = null)
        {
            Vector3D targetAngle;
            WpInfo wiTAngle = new WpInfo();
            if (wi != null)
                wiTAngle = wi;
            else
                wi = wpInfo;

            wiTAngle.direction = wi.direction;
            wiTAngle.angle = Vector3D.Normalize(GetPos() - wi.pos);
            Vector3D target = wi.pos;
            double stoppingDist = (wi.stopDist == 0 ? settings.StoppingDistance : wi.stopDist),
                    thrust = (wi.thrust == 0 ? settings.DefaultThrust : wi.thrust),
                    ovrSpeed = (wi.speed == 0 ? settings.MaxSpeed : wi.speed);
            double maxSpeed, distToDest, maxThrust, breakDist, glideUpperThreshold, glideLowerThreshold;

            List<IMyTerminalBlock> thrusters = fwdThrusters, inverseThrusters = rvsThrusters;
            distToDest = (target - GetPos()).Length();
            if (wi.direction == "reverse")
            {
                thrusters = rvsThrusters;
                inverseThrusters = fwdThrusters;
            }
            else if (wi.direction == "left")
            {
                thrusters = leftThrusters;
                inverseThrusters = rightThrusters;
            }
            else if (wi.direction == "right")
            {
                thrusters = rightThrusters;
                inverseThrusters = leftThrusters;
            }
            else if (wi.direction == "up")
            {
                thrusters = upThrusters;
                inverseThrusters = downThrusters;
            }
            else if (wi.direction == "down")
            {
                thrusters = downThrusters;
                inverseThrusters = upThrusters;
            }

            maxThrust = thrust;
            breakDist = ovrSpeed * settings.BreakingDistMultiplier;

            vs.SetVarToStore("Dist to go", distToDest.ToString());

            bool inBreakingZone = distToDest < breakDist;
            if (inBreakingZone && !wi.noStop)
            {
                maxSpeed = (Math.Log(((distToDest + (breakDist * 0.0175)) / breakDist) * 100, 700) * ovrSpeed) - (ovrSpeed * 0.1);
                maxSpeed = maxSpeed < 1.5 ? 1.5 : maxSpeed;
                vs.SetVarToStore("adjustedMaxSpeed", maxSpeed.ToString());
                thrust = distToDest > 10 ? thrust : thrust / 2;
                maxThrust = thrust * (maxSpeed / ovrSpeed);
                maxThrust = maxThrust < settings.MinThrust ? settings.MinThrust : maxThrust;
                maxThrust = avgSpeed < 0.2 ? 20 : maxThrust;
                glideUpperThreshold = 0.80;
                glideLowerThreshold = 0.50;

            }
            else
            {
                vs.DelVarFromStore("adjustedMaxSpeed");
                maxSpeed = ovrSpeed;
                double thrustModifier = (maxSpeed - avgSpeed) / maxSpeed;
                maxThrust = thrust * (thrustModifier > 0.5 ? 1 : thrustModifier);
                glideUpperThreshold = 1;
                glideLowerThreshold = 0.95;

            }

            double fwdThrust = 0, rvsThrust = 0;
            bool dampening = true, arrived = false;

            if (distToDest > stoppingDist)
            {
                double thrustModifier = (maxSpeed - avgSpeed) / maxSpeed;

                if (avgSpeed < -0.001)
                {
                    // We are going backwards
                    dampening = true;
                    rvsThrust = 0;
                    fwdThrust = 0;
                }
                else if (avgSpeed >= -0.001 && avgSpeed < maxSpeed * glideLowerThreshold)
                {
                    // We are between 0% and 90% avgSpeed
                    dampening = true;
                    rvsThrust = 0;
                    fwdThrust = maxThrust;
                }
                else if (avgSpeed >= maxSpeed * glideLowerThreshold && avgSpeed <= maxSpeed * glideUpperThreshold)
                {
                    // we are between 90% and 100% avgSpeed
                    dampening = false;
                    rvsThrust = 0;
                    fwdThrust = 0;
                }
                else if (avgSpeed > maxSpeed * glideUpperThreshold)
                {
                    // we are exceeding the Speed
                    var overSpeedAmount = (avgSpeed - (maxSpeed));
                    var rvsThrustModifier = (overSpeedAmount > 0 ? (overSpeedAmount) / ((maxSpeed * 0.5)) : 0.15) + 0.1;
                    if ((rvsThrustModifier >= 1.1))
                    {
                        // we are exceeding more than 50%
                        //LCD.dbug("Emergency brakes activated\n-over avgSpeed by:" + overSpeedAmount + "(" + (rvsThrustModifier * 100) + "%)\n" + "SP:" + avgSpeed + "Max:" + maxSpeed * glideUpperThreshold + " / " + maxSpeed, 2);
                        dampening = true;
                        rvsThrust = 0;
                        fwdThrust = 0;
                    }
                    else if (rvsThrustModifier < 1.1)
                    {
                        // we are exceeding less than 50%
                        dampening = true;
                        rvsThrust = thrust * (rvsThrustModifier > 0.70 ? 1 : rvsThrustModifier);
                        fwdThrust = 0;
                    }
                }

                if (!align(wiTAngle, false))
                {
                    vs.SetVarToStore("ReAligning", "true");
                    if (wpInfo.alignFirst)
                    {
                        rvsThrust = 0;
                        fwdThrust = 0;
                    }
                    else if (fwdThrust == 0 && rvsThrust == 0)
                        fwdThrust = 2;
                    dampening = true;
                }
                else
                {
                    wpInfo.alignFirst = false;
                    vs.SetVarToStore("ReAligning", "false");
                }
            }
            else
            { arrived = true; Gyro.YawStop(gyros); Gyro.PitchStop(gyros); Gyro.RollStop(gyros); Gyro.GyroOverride(gyros, false); if (wi.noStop)dampening = false; }
            Thruster.SetThrustOverride(thrusters, fwdThrust);
            Thruster.SetThrustOverride(inverseThrusters, rvsThrust);
            Cockpit.TurnOnOffDampeners(controller, dampening);
            return arrived;
        }

        public double DotProduct(Vector3D v1, Vector3D v2) { return v1.GetDim(0) * v2.GetDim(0) + v1.GetDim(1) * v2.GetDim(1) + v1.GetDim(2) * v2.GetDim(2); }

        public Vector3D GetPos()
        {
            if (settings.CentreBlock == "rear")
                return rearPos;
            else if (settings.CentreBlock == "fwd")
                return fwdPos;
            else if (settings.CentreBlock == "top")
                return tPos;
            else if (settings.CentreBlock == "bot")
                return bPos;
            else if (settings.CentreBlock == "left")
                return lPos;
            else if (settings.CentreBlock == "right")
                return rPos;

            return rearPos;
        }

        public Vector3D ShipUp()
        {
            return Vector3D.Normalize(tPos - bPos);
        }
        public Vector3D ShipLeft()
        {
            return Vector3D.Normalize(lPos - rPos);
        }
        public Vector3D GetAngle()
        {
            return Vector3D.Normalize(rearPos - fwdPos);
        }
        public Vector3D GetCross()
        {
            return Vector3D.Normalize(Vector3D.Cross(bPos - tPos, fwdPos - rearPos));
        }
        public Vector3D GetOffset(double offset, string direction = "left")
        {
            if (direction == "left")
                return new Vector3D(GetPos() - (Vector3D.Normalize(rPos - lPos) * offset));
            else if (direction == "right")
                return new Vector3D(GetPos() + (Vector3D.Normalize(rPos - lPos) * offset));
            else if (direction == "up")
                return new Vector3D(GetPos() - (Vector3D.Normalize(bPos - tPos) * offset));
            else if (direction == "down")
                return new Vector3D(GetPos() + (Vector3D.Normalize(bPos - tPos) * offset));
            else if (direction == "forward")
                return new Vector3D(GetPos() - (Vector3D.Normalize(rearPos - fwdPos) * offset));
            else if (direction == "backward")
                return new Vector3D(GetPos() + (Vector3D.Normalize(rearPos - fwdPos) * offset));

            return new Vector3D();
        }
        public void AdjustSpeed(double speed)
        {
            if (wpInfo == null)
                return;
            wpInfo.speed = speed;
        }
    }  
}
