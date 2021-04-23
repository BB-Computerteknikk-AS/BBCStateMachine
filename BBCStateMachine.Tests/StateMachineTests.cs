using System;
using System.Threading;
using NLog;
using Xunit;

namespace no.bbc.StateMachine
{
    public class StateMachineTests
    {
        public StateMachineTests()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Tests the stack machine using the GarageDoorController
        /// 1. Opens the doors
        /// 2. Waits for the doors to be fully opened
        /// 3. Closes the doors
        /// 4. Waits for the doors to be fully closed
        /// </summary>
        [Fact(DisplayName = "Test State Machine Functionality")]
        public void TestStackMachine()
        {
            AutoResetEvent doorsOpenedHandle = new AutoResetEvent(false);
            AutoResetEvent doorsClosedHandle = new AutoResetEvent(false);

            var garageDoorController = new GarageDoorController();

            garageDoorController.OnDoorsOpened += () =>
            {
                doorsOpenedHandle.Set();
            };

            garageDoorController.OnDoorsClosed += () =>
            {
                doorsClosedHandle.Set();
            };

            garageDoorController.OpenDoors();

            doorsOpenedHandle.WaitOne(3000);

            garageDoorController.CloseDoors();

            doorsClosedHandle.WaitOne(3000);
        }

        /// <summary>
        /// Compile a dot graph for visual representation
        /// </summary>
        [Fact(DisplayName = "Compile State Machine dot Graph")]
        public void CompileDotGraph()
        {
            var garageDoorController = new GarageDoorController();

            var possibleTransitions = garageDoorController._stateMachine.GetPossibleTransitions();

            var unhandledTransitions = garageDoorController._stateMachine.GetUnhandledTransitions();

            var unhandledInputs = garageDoorController._stateMachine.GetUnhandledInputs();

            var dotGraph = garageDoorController.GraphCompiler.CompileDotGraph();

            var dotGraphWithUnhandledTransitions = garageDoorController.GraphCompiler.CompileDotGraph(true);

            // results can be visualized here: http://graphviz.it
            Assert.NotNull(dotGraph);

            Console.WriteLine(dotGraph);
        }
    }
}
