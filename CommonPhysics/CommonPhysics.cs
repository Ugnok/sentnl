using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    public partial class Program : MyGridProgram
    {
        public double calcTime(double vI, double vF, double a)
        {
            double time = (vF - vI) / a;
            if (time < 0)
                throw new Exception("Sign mismatch between velocities and acceleration.");
            return time;
        }

        public double calcDistance(double vI, double a, double t)
        {
            return vI * t + (a * t * t) / 2;
        }
    }
}
