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
    partial class Program : MyGridProgram
    {
        IMyThrust thrusterForward, thrusterBackward;
        IMyInteriorLight light;
        IMyShipController controller;
        IEnumerator<bool> _stateMachine;

        int target = -1027;

        public Program()
        {
            // Retrieve the blocks we're going to use.
            thrusterForward = GridTerminalSystem.GetBlockWithName("Ion Thrusters") as IMyThrust;
            thrusterBackward = GridTerminalSystem.GetBlockWithName("Ion Thrusters 2") as IMyThrust;
            light = GridTerminalSystem.GetBlockWithName("Interior Light") as IMyInteriorLight;
            controller = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;

            // Initialize our state machine
            _stateMachine = RunStuffOverTime();

            // Signal the programmable block to run again in the next tick. Be careful on how much you
            // do within a single tick, you can easily bog down your game. The more ticks you do your
            // operation over, the better.
            //
            // What is actually happening here is that we are _adding_ the Once flag to the frequencies.
            // By doing this we can have multiple frequencies going at any time.
            Runtime.UpdateFrequency |= UpdateFrequency.Once;
        }

        public void Main(string argument, UpdateType updateType)
        {
            // Usually I verify that the argument is empty or a predefined value before running the state
            // machine. This way we can use arguments to control the script without disturbing the
            // state machine and its timing. For the purpose of this example however, I will omit this.

            // We only want to run the state machine(s) when the update type includes the
            // "Once" flag, to avoid running it more often than it should. It shouldn't run
            // on any other trigger. This way we can combine state machine running with
            // other kinds of execution, like tool bar commands, sensors or what have you.
            if ((updateType & UpdateType.Once) == UpdateType.Once)
            {
                RunStateMachine();
            }
        }

        // ***MARKER: State Machine Execution
        public void RunStateMachine()
        {
            // If there is an active state machine, run its next instruction set.
            if (_stateMachine != null)
            {
                // The MoveNext method is the most important part of this system. When you call
                // MoveNext, your method is invoked until it hits a `yield return` statement.
                // Once that happens, your method is halted and flow control returns _here_.
                // At this point, MoveNext will return `true` since there's more code in your
                // method to execute. Once your method reaches its end and there are no more
                // yields, MoveNext will return false to signal that the method has completed.
                // The actual return value of your yields are unimportant to the actual state
                // machine.

                // If there are no more instructions, we stop and release the state machine.
                if (!_stateMachine.MoveNext())
                {
                    _stateMachine.Dispose();

                    // In our case we just want to run this once, so we set the state machine
                    // variable to null. But if we wanted to continously run the same method, we
                    // could as well do
                    // _stateMachine = RunStuffOverTime();
                    // instead.
                    _stateMachine = null;
                }
                else
                {
                    // The state machine still has more work to do, so signal another run again, 
                    // just like at the beginning.
                    Runtime.UpdateFrequency |= UpdateFrequency.Once;
                }
            }
        }

        // ***MARKER: State Machine Program
        // The return value (bool in this case) is not important for this example. It is not
        // actually in use.
        public IEnumerator<bool> RunStuffOverTime()
        {
            light.Enabled = false;
            thrusterForward.ThrustOverride = thrusterForward.MaxThrust;
            yield return true;

            // The following would seemingly be an illegal operation, because the script would
            // keep running until the instruction count overflows. However, using yield return,
            // you can get around this limitation - without breaking the rules and while remaining
            // performance friendly.
            bool forward = true;
            while (forward)
            {
                // Like before, when this statement is executed, control is returned to the game.
                // This way you can have a continuously polling script with complete state
                // management, with very little effort.

                Echo("My position: " + controller.GetPosition());

                // Calculate maximum stopping acceleration
                double stopForce = thrusterBackward.MaxThrust;
                Echo("Stop force: " + stopForce);
                double stopAccel = stopForce / controller.CalculateShipMass().PhysicalMass;
                Echo("Total mass: " + controller.CalculateShipMass().TotalMass);
                Echo("Physical mass: " + controller.CalculateShipMass().PhysicalMass);
                Echo("Stop acceleration: " + stopAccel);

                // Calculate time to stop
                double timeToStop = calcTime(controller.GetShipVelocities().LinearVelocity.X, 0, stopAccel);
                //double timeToStop = calcTime(-20, 0, stopAccel);
                Echo("Velocity: " + controller.GetShipVelocities().LinearVelocity.X);
                Echo("Time to stop: " + timeToStop);

                // Calculate distance needed to stop
                double distanceToStop = calcDistance(controller.GetShipVelocities().LinearVelocity.X, stopAccel, timeToStop);
                Echo("Distance to stop: " + distanceToStop);

                double distanceToTarget = target - controller.GetPosition().X;
                Echo("Distance to target: " + distanceToTarget);

                if (Math.Abs(distanceToTarget) <= Math.Abs(distanceToStop))
                {
                    thrusterForward.ThrustOverride = 0;
                    thrusterBackward.ThrustOverride = thrusterBackward.MaxThrust;
                    light.Enabled = true;
                    forward = false;
                }
                yield return true;
            }
            // braking
            while (true)
            {
                Echo("My position: " + controller.GetPosition());

                // stopped
                if (controller.GetShipVelocities().LinearVelocity.X >= 0)
                {
                    thrusterBackward.ThrustOverride = 0;
                    light.Enabled = false;
                    controller.DampenersOverride = true;
                    yield return false;
                }

                yield return true;
            }
        }
    }
}