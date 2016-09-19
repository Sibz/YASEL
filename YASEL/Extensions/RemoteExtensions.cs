using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace RemoteExtensions
{
    public static class RemoteExtensions
    {
        public static void SetDirection(this IMyRemoteControl remote, Base6Directions.Direction direction)
        {
            switch(direction)
            {
                case Base6Directions.Direction.Forward:
                    remote.GetActionWithName("Forward").Apply(remote);
                    return;
                case Base6Directions.Direction.Backward:
                    remote.GetActionWithName("Backward").Apply(remote);
                    return;
                case Base6Directions.Direction.Up:
                    remote.GetActionWithName("Up").Apply(remote);
                    return;
                case Base6Directions.Direction.Down:
                    remote.GetActionWithName("Down").Apply(remote);
                    return;
                case Base6Directions.Direction.Left:
                    remote.GetActionWithName("Left").Apply(remote);
                    return;
                case Base6Directions.Direction.Right:
                    remote.GetActionWithName("Right").Apply(remote);
                    return;
            }
        }
    }
}