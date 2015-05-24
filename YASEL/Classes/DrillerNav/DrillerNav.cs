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
using VRageMath;


namespace YASEL
{
    partial class Program
    {
        #region DrillerNav
        //# Requires VarStore
        //# Requires DrillerNavSettings
        //# Requires NavManager
        //# Requires LCDFunctions
        //# Requires BlockFunctions
        //# Requires BatteryFunctions
        //# Requires ConnectorFunctions

        // Version 0.6.0

        public const int STATE_RESET = 0;
        public const int STATE_RUNNING = 1;
        public const int STATE_WPMODE = 2;
        public const int STATE_MININGALIGN = 3;

        public const string DN_ERROR_1000 = "Error DN1000: Driller nav could not be initialised as a global NavManager is not available";
        public const string DN_ERROR_1002 = "Error DN1002: Driller nav could not be initialised, unable to acquire reference to a LCD for waypoints check name and that it exists";


        public class DrillerNav
        {
            int state, curShaftCount;

            VarStore vs;

            DrillerNavSettings settings;

            WpInfo wpInfo, wpRoid;

            Dictionary<int, WpInfo> wpiUndock, wpiExitHangar, wpiToRoid, wpiMine, wpiRTB, wpiDock;

            double shaftX, shaftY;

            bool firstRun, mining, enounteredRoidInShaft;

            Vector3D curShaftStart, curShaftEnd;

            IMyTextPanel lcdWaypoints;

            public DrillerNav(DrillerNavSettings s = null)
            {
                settings = s == null ? new DrillerNavSettings() : s;


                vs = new VarStore(s.LCDVariableStoreName);

                dbug("Initialising DrillerNav", 2);

                if (nm == null)
                    throw new Exception(DN_ERROR_1000);

                lcdWaypoints = GetBlock(settings.LCDWaypointsName) as IMyTextPanel;

                if (lcdWaypoints == null)
                { dbug(DN_ERROR_1002); throw new Exception(DN_ERROR_1002); }

                wpiUndock = new Dictionary<int, WpInfo>();
                wpiExitHangar = new Dictionary<int, WpInfo>();
                wpiToRoid = new Dictionary<int, WpInfo>();
                wpiMine = new Dictionary<int, WpInfo>();
                wpiRTB = new Dictionary<int, WpInfo>();
                wpiDock = new Dictionary<int, WpInfo>();
                wpiUndock.Add(wpiUndock.Count, new WpInfo() { wpId = wpiUndock.Count + 1, enabled = true, stopDist = 4.5, type = "travel", direction = "left", pos = new Vector3D(-121.507127546412, -254.517746679787, -132.807750221742) });
                wpiUndock.Add(wpiUndock.Count, new WpInfo() { wpId = wpiUndock.Count + 1, enabled = true, stopDist = 5, type = "travel", direction = "reverse", pos = new Vector3D(-121.256452883996, -243.900884773361, -129.076239941945) });
                wpiUndock.Add(wpiUndock.Count, new WpInfo() { wpId = wpiUndock.Count + 1, enabled = true, type = "align-level-wait", angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1), waitTime = 25, CallbackMethod = undockComplete });
                wpiExitHangar.Add(wpiExitHangar.Count, new WpInfo() { wpId = wpiExitHangar.Count + 1, stopDist = 25, enabled = true, type = "travel", pos = new Vector3D(-121.153897841384, -172.556658831972, -128.994433661708), direction = "reverse", CallbackMethod = exitHangarComplete });
                /*1:enabled=True;type=wp;pos=-129.21333901392,-239.402925465687,-315.489090637876
                2:enabled=True;type=wp;pos=-18049.7946270778,5934.87664958981,-26592.6990209968
                5:enabled=True;type=wp;pos=-18042.6353919198,5981.21422348576,-26412.9293718236*/

                wpiToRoid.Add(wpiToRoid.Count, new WpInfo() { stopDist = 10, type = "travel", pos = new Vector3D(-129.21333901392, -239.402925465687, -315.489090637876) });
                wpiToRoid.Add(wpiToRoid.Count, new WpInfo() { stopDist = 0.25, type = "travel", pos = new Vector3D(-18049.7946270778, 5934.87664958981, -26592.6990209968), CallbackMethod = toRoidComplete });
                wpiRTB.Add(wpiRTB.Count, new WpInfo() { wpId = wpiRTB.Count + 1, stopDist = 10, type = "travel", pos = new Vector3D(-18049.7946270778, 5934.87664958981, -26592.6990209968) });
                wpiRTB.Add(wpiRTB.Count, new WpInfo() { wpId = wpiRTB.Count + 1, type = "travel", stopDist = 1, pos = new Vector3D(-129.21333901392, -239.402925465687, -315.489090637876) });
                wpiRTB.Add(wpiRTB.Count, new WpInfo() { wpId = wpiRTB.Count + 1, type = "travel", stopDist = 1, pos = new Vector3D(-121.002199852628, -222.779268153978, -128.703693788851) });
                wpiRTB.Add(wpiRTB.Count, new WpInfo() { wpId = wpiRTB.Count + 1, type = "align-level-wait", angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1), waitTime = 20, CallbackMethod = rtbComplete });
                wpiDock.Add(wpiDock.Count, new WpInfo() { wpId = wpiDock.Count + 1, stopDist = 0.85, type = "travel", pos = new Vector3D(-121.544733468484, -254.514494468742, -128.833965646119) });
                wpiDock.Add(wpiDock.Count, new WpInfo() { wpId = wpiDock.Count + 1, type = "align-level", angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1) });
                wpiDock.Add(wpiDock.Count, new WpInfo() { wpId = wpiDock.Count + 1, stopDist = 0.25, type = "travel", direction = "right", pos = new Vector3D(-121.539200202866, -254.519797052459, -127.089228373509) });
                wpiDock.Add(wpiDock.Count, new WpInfo() { wpId = wpiDock.Count + 1, type = "align-level", angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1), CallbackMethod = dockComplete });
                wpRoid = new WpInfo() { wpId = wpiDock.Count + 1, stopDist = 0.25, type = "travel-align-level", pos = new Vector3D(-18042.6353919198, 5981.21422348576, -26412.9293718236), angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1) };
                firstRun = true;
                curShaftCount = settings.StartingShaftNumber;
                curShaftStart = new Vector3D(0, 0, 0);
                dbug("", 1, false);

            }

            public void Tick()
            {
                nm.Tick();
                dbug("DrillerNav Tick():", 2);
                if (state == STATE_RUNNING)
                {
                    operate();
                }
                else if (state == STATE_MININGALIGN)
                {
                    alignForMining();
                }
                else
                {
                   
                }
            }

            private void operate()
            {
                
                    nm.UnPause();

                    // adjust speed depending on if there is an asteroid directly infront (only when mining)
                    if (asteroidInFront() && mining)
                    { nm.AdjustSpeed(settings.MiningSpeed); enounteredRoidInShaft = true; }
                    else if (mining)
                        nm.AdjustSpeed(0); // default speed in NM;

                    // If NavManager is doing something and not full/lowBat while mining, return
                    if (nm.IsActing() && !((cargoFull() || batteryLow()) && mining))
                        return;

                    // Stop mining and rtb if cargo full or battery low
                    if (((cargoFull() || batteryLow()) && mining))
                    { stopMiningRtb(); return; }

                    // Only continue to code below if firstRun
                    if (!firstRun)
                        return;

                    // This is our start point
                    //undock();
                    //toRoid();
                    //mineShaft();
                    rtb();

                    firstRun = false;
                
            }
            private bool asteroidInFront()
            {
                var sensor = GetBlock(settings.AsteroidSensorName);
                if (!(sensor is IMySensorBlock))
                    return true; // if its not a sensor block or doesn't exist, by default we assume theres an asteroid infront.
                if ((sensor as IMySensorBlock).IsActive)
                    return true;
                return false;
            }


            private double cargoPercent()
            {
                double curAmount = 0, maxAmount = 0;
                var invBlocks = GetBlockGrp(settings.OreCargoGroupName);
                invBlocks.ForEach(b =>
                {
                    IMyInventory inv = (b as IMyInventoryOwner).GetInventory(0);
                    maxAmount += (double)(inv.MaxVolume);
                    curAmount += (double)(inv.CurrentVolume);
                });
                return Math.Round(curAmount / maxAmount * 100, 2);
            }
            private bool cargoFull() { if (cargoPercent() >= 99)return true; dbug("cargo not full", 3); return false; }
            private bool cargoEmpty() { if (cargoPercent() <= 1)return true; dbug("cargo not empty", 3); return false; }

            private double batteryPercent()
            {
                double curCharge = 0, maxCharge = 0;
                var batteries = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyBatteryBlock>(batteries, BelongsToGrid);
                //dbug("batteries:"+GetBatteryCharge(batteries)+"%");
                return Math.Round(GetBatteryCharge(batteries) * 100, 2);
            }
            private bool batteryLow() { if (batteryPercent() <= 10)return true; dbug("bat not low", 3); return false; }
            private bool batteryFull() { if (batteryPercent() >= 99)return true; dbug("bat not full", 3); return false; }

            private void undock()
            {
                dbug("Undocking");
                // No roid, no go
                if (wpRoid == null)
                {
                    state = STATE_RESET;
                    nm.Reset();
                    reset();
                    return;
                }
                // TODO: check position
                // 
                // Start Reactors
                // unlock and power down connectors
                var connectors = new List<IMyTerminalBlock>();
                var reactors = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipConnector>(connectors, BelongsToGrid);
                //dbug("Undocking, found " + connectors.Count + " Connectors");
                //throw new Exception("Was undocking: found " + connectors.Count + " Connectors");
                gts.GetBlocksOfType<IMyReactor>(reactors, BelongsToGrid);
                TurnOnOff(reactors, true);
                TurnOnOff(connectors, false);
                // Act on leave wpinfo list
                nm.Act(wpiUndock);

            }
            public void undockComplete()
            {
                dbug("Undock Complete");
                // Turn connectors back on
                var connectors = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipConnector>(connectors, BelongsToGrid);
                TurnOnOff(connectors, true);
                // Exit hangar
                exitHangar();
            }
            private void exitHangar()
            { nm.Act(wpiExitHangar); }
            private void exitHangarComplete()
            { toRoid(); }
            private void toRoid()
            {
                dbug("To Roid");
                nm.Act(wpiToRoid);
            }
            private void toRoidComplete()
            { dbug("To Roid Complete"); mineShaft(); }
            private void mineShaft()
            {
                dbug("Mining Shaft");
                // Settings:
                // shaftWidth
                // shaftHeight
                // shaftMaxCount
                // shaftDepth
                //
                // classwide vars (remembered each exec)
                // curShaftCount
                // curShaftStart - beggining of the current shaft
                // curShaftEnd - end of current shaft

                int x, y;
                enounteredRoidInShaft = false;
                // get roid position, offset 5 metres
                curShaftStart = wpRoid.pos - (wpRoid.angle * 5);
                // Get position in spiral
                getSpiralXY(curShaftCount, settings.ShaftMaxCount * settings.ShaftMaxCount, out x, out y);
                dbug("Got Sprial: count: " + curShaftCount + " x/y: " + x + "/" + y, 2);
                curShaftStart = curShaftStart + (nm.ShipUp() * (x * settings.ShaftHeight));
                curShaftStart = curShaftStart + (nm.ShipLeft() * (y * settings.ShaftWidth));
                curShaftEnd = curShaftStart - (wpRoid.angle * settings.ShaftDepth);
                var wpInfoStart = new WpInfo() { thrust = 20, type = "travel-align-level", alignFirst = true, speed = 5, stopDist = 0.25, pos = curShaftStart, angle = wpRoid.angle, cross = wpRoid.cross };
                var wpInfoMine = new WpInfo() { thrust = 20, type = "travel-align-level", alignFirst = true, speed = settings.MiningSpeed, stopDist = 5, pos = curShaftEnd, angle = wpRoid.angle, cross = wpRoid.cross, CallbackMethod = mineShaftComplete };

                mining = true;

                var drills = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipDrill>(drills);
                TurnOnOff(drills);

                // Move to start of shaft, with as much accuracy as possible
                nm.Act(wpInfoStart); // This should be new centreIn waypoint when done.
                nm.Act(wpInfoMine);

            }
            private void mineShaftComplete()
            {
                dbug("Mining Shaft Complete");
                var drills = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipDrill>(drills);
                TurnOnOff(drills, false);
                mining = false;
                curShaftCount++;
                var wpInfoReturn = new WpInfo() { thrust = 20, type = "travel", direction = "reverse", speed = enounteredRoidInShaft ? 2.5 : 50, stopDist = 45, pos = curShaftStart + (wpRoid.angle * 50) };

                if (curShaftCount > settings.ShaftMaxCount * settings.ShaftMaxCount)
                {
                    // we have completed this roid
                    //
                    dbug("roidComplete");
                    wpRoid = null;
                    wpInfoReturn.CallbackMethod = rtb;
                }
                else
                    wpInfoReturn.CallbackMethod = mineShaft;

                nm.Act(wpInfoReturn);

            }
            private void stopMiningRtb()
            {

                var drills = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipDrill>(drills);
                TurnOnOff(drills, false);
                mining = false;
                dbug("Stopping mining, rtb");
                nm.Reset();
                var wpInfoReturn = new WpInfo() { thrust = 20, type = "travel", direction = "reverse", speed = 2.5, stopDist = 45, pos = curShaftStart + (wpRoid.angle * 50) };
                wpInfoReturn.CallbackMethod = rtb;
                nm.Act(wpInfoReturn);
            }

            private void rtb()
            { dbug("rtb"); nm.Act(wpiRTB); }
            private void rtbComplete()
            { dock(); }
            private void dock()
            {
                dbug("Docking");
                nm.Act(wpiDock);
            }
            private void dockComplete()
            {
                var connectors = new List<IMyTerminalBlock>();
                gts.GetBlocksOfType<IMyShipConnector>(connectors, BelongsToGrid);

                var wpWait = new WpInfo() { type = "wait", waitTime = 1, CallbackMethod = dockComplete };
                if (!IsReadyToLock(connectors))
                    nm.Act(wpWait);

                SwitchLock(connectors);
                waitEmpty();
            }
            private void waitEmpty()
            {
                var wpWait = new WpInfo() { type = "wait", waitTime = 1, CallbackMethod = waitEmpty };
                if (!cargoEmpty() || !batteryFull())
                    nm.Act(wpWait);
                else
                    undock();
            }

            private void getSpiralXY(int p, int n, out int X, out int Y)
            {
                dbug("Position: " + p);
                int positionX = 0, positionY = 0, direction = 0, stepsCount = 1, stepPosition = 0, stepChange = 0;
                X = 0;
                Y = 0;
                for (int i = 0; i < n * n; i++)
                {
                    if (i == p)
                    {
                        X = positionX;
                        Y = positionY;
                        return;
                    }
                    if (stepPosition < stepsCount)
                    {
                        stepPosition++;
                    }
                    else
                    {
                        stepPosition = 1;
                        if (stepChange == 1)
                        {
                            stepsCount++;
                        }
                        stepChange = (stepChange + 1) % 2;
                        direction = (direction + 1) % 4;
                    }
                    if (direction == 0) { positionY++; }
                    else if (direction == 1) { positionX--; }
                    else if (direction == 2) { positionY--; }
                    else if (direction == 3) { positionX++; }
                }
            }

            private void reset()
            {
                string curText = lcdWaypoints.GetPublicText();
                curText = curText.Replace("complete=true;", "");
                lcdWaypoints.WritePublicText(curText, false);
                lcdWaypoints.ShowPublicTextOnScreen();
                wpInfo = null;
                firstRun = true;
            }

            private void alignForMining()
            {
                if (nm.IsActing())
                    return;

                if (wpInfo != null)
                {
                    // finished acting on WP
                    wpInfo = null;
                    state = STATE_RESET;
                }
                else
                {
                    // WP is null and we can start nm acting
                    dbug("Starting to align/level");
                    wpInfo = new WpInfo() { type = "level-align", angle = new Vector3D(0, 1, 0), cross = new Vector3D(0, 0, 1) };//pos=new Vector3D(0,1,0);
                    wpInfo.AlignTo();
                    wpInfo.LevelTo();
                    dbug("Starting to act");
                    nm.Act(wpInfo);
                }
            }
        }

        
        #endregion
    }
}
