using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace YaNavControl
{
    using ProgramExtensions;
    using GyroExtensions;
    using ThrustExtensions;
    using BlockExtensions;
    using YaNavThrusterControl;
    using YaNav;
    class YaNavControl
    {
         //
        //Class Variables
        //
        MyGridProgram gp;
        YaNavSettings settings;

        bool debugOn = true;

        //Blocks
        IMyTerminalBlock remote;

        //Controllers

        YaNavGyroControl gyroscopeController;
        YaNavThrusterControl thrusterController;

        //Warning storage
        List<string> warnings;

        //Waypoint Storage
        List<YaNavWaypoint> waypoints;

        public YaNavControl(MyGridProgram gp, YaNavSettings s)
        {
            this.gp = gp;
            settings = s;

            debug("Initialising YaNav");
            
            waypoints = new List<YaNavWaypoint>();

            debug("-Setting up remote");
            setupRemote();
            debug("-setting up gyros controller");
            gyroscopeController = new YaNavGyroControl(gp);
            debug("-Initialisation complete");
        }
        private void setupRemote()
        {
            debug(" -Getting remote");
            if (settings.RemoteName != "")
                remote = gp.GetBlock(settings.RemoteName);
            else
            {
                debug(" -finding remote");
                var remotes = new List<IMyTerminalBlock>();
                gp.GridTerminalSystem.GetBlocksOfType<IMyRemoteControl>(remotes, b => { return b.CubeGrid == gp.Me.CubeGrid; });
                if (remotes.Count > 0)
                    remote = remotes[0];
            }
            if (remote == null)
                throw new YaNavException(lang.ErrorNoRemote);

            debug(" -Setup remote complete");
        }
        private void addWarning(string warning)
        {
            if (!warnings.Contains(warning)) warnings.Add(warning);
        }
        
        private void debug(string message)
        {
            if (debugOn) gp.Echo(message);
        }
    }
    

    public class YaNavWaypoint
    {

    }

    public class YaNavSettings
    {
        public string RemoteName = "";
        public bool InNatrualGravityOnly = false;
        public YaNavThrusterGroupNames ThrusterGroupNames;


    }
   
}