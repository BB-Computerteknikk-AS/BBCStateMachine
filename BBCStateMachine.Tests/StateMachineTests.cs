using System;
using System.Threading;
using System.Threading.Tasks;
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

        class GarageDoorController
        {
            public enum GarageDoorState
            {
                Opening,
                Opened,
                Closing,
                Closed,
                Stopped
            }

            public enum GarageDoorAction
            {
                Open,
                Close,
                OnOpened,
                OnClosed,
            }

            #region Public Constructor

            public GarageDoorController()
            {
                _stateMachine = new StateMachine<GarageDoorState, GarageDoorAction>(GarageDoorState.Closed);

                _stateMachine.RegisterTransition(GarageDoorState.Opening, GarageDoorAction.Open, GarageDoorState.Stopped);
                _stateMachine.RegisterTransition(GarageDoorState.Opening, GarageDoorAction.Close, GarageDoorState.Closing);
                _stateMachine.RegisterTransition(GarageDoorState.Opening, GarageDoorAction.OnOpened, GarageDoorState.Opened);

                _stateMachine.RegisterTransition(GarageDoorState.Opened, GarageDoorAction.Open, GarageDoorState.Opened);
                _stateMachine.RegisterTransition(GarageDoorState.Opened, GarageDoorAction.Close, GarageDoorState.Closing);

                _stateMachine.RegisterTransition(GarageDoorState.Closing, GarageDoorAction.Close, GarageDoorState.Stopped);
                _stateMachine.RegisterTransition(GarageDoorState.Closing, GarageDoorAction.Open, GarageDoorState.Opening); ;
                _stateMachine.RegisterTransition(GarageDoorState.Closing, GarageDoorAction.OnClosed, GarageDoorState.Closed);

                _stateMachine.RegisterTransition(GarageDoorState.Closed, GarageDoorAction.Open, GarageDoorState.Opening);
                _stateMachine.RegisterTransition(GarageDoorState.Closed, GarageDoorAction.Close, GarageDoorState.Closed);

                _stateMachine.RegisterTransition(GarageDoorState.Stopped, GarageDoorAction.Open, GarageDoorState.Opening);
                _stateMachine.RegisterTransition(GarageDoorState.Stopped, GarageDoorAction.Close, GarageDoorState.Closing);
            }

            #endregion

            #region Internal Fields

            internal StateMachine<GarageDoorState, GarageDoorAction> _stateMachine;

            #endregion

            #region Public Properties            

            public StateMachineGraphCompiler<GarageDoorState, GarageDoorAction> GraphCompiler
            {
                get
                {
                    return new StateMachineGraphCompiler<GarageDoorState, GarageDoorAction>(_stateMachine);
                }
            }

            public event Action OnDoorsOpened;
            public event Action OnDoorsClosed;

            #endregion

            #region Public Methods

            public void Initialize()
            {
                _stateMachine.OnEnterState(GarageDoorState.Opening, (prevState, newState, input) =>
                {
                    // simulate time consuiming work
                    Task.Delay(1000).ContinueWith((t) =>
                    {
                        _stateMachine.HandleInput(GarageDoorAction.OnOpened);
                    });
                });

                _stateMachine.OnEnterState(GarageDoorState.Closing, (prevState, newState, input) =>
                {
                    // simulate time consuiming work
                    Task.Delay(1000).ContinueWith((t) =>
                    {
                        _stateMachine.HandleInput(GarageDoorAction.OnClosed);
                    });
                });

                _stateMachine.OnEnterState(GarageDoorState.Opened, (prevState, newState, input) =>
                {
                    OnDoorsOpened?.Invoke();
                });

                _stateMachine.OnEnterState(GarageDoorState.Closed, (prevState, newState, input) =>
                {
                    OnDoorsClosed?.Invoke();
                });
            }

            public void OpenDoors()
            {
                _stateMachine.HandleInput(GarageDoorAction.Open);
            }

            public void CloseDoors()
            {
                _stateMachine.HandleInput(GarageDoorAction.Close);
            }

            #endregion

            #region Private Methods

            #endregion
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

            garageDoorController.Initialize();
            garageDoorController.OpenDoors();

            doorsOpenedHandle.WaitOne(3000);

            garageDoorController.CloseDoors();

            doorsClosedHandle.WaitOne(3000);
        }

        [Fact(DisplayName = "Ignore Same State Transitions")]
        public void IgnoreSameStateTransitions()
        {
            StateMachine<GarageDoorController.GarageDoorState, GarageDoorController.GarageDoorAction> stateMachine = new StateMachine<GarageDoorController.GarageDoorState, GarageDoorController.GarageDoorAction>(GarageDoorController.GarageDoorState.Closed);
            stateMachine.RegisterTransition(GarageDoorController.GarageDoorState.Closed, GarageDoorController.GarageDoorAction.Open, GarageDoorController.GarageDoorState.Opening);
            stateMachine.RegisterTransition(GarageDoorController.GarageDoorState.Opening, GarageDoorController.GarageDoorAction.Open, GarageDoorController.GarageDoorState.Opening);

            stateMachine.HandleInput(GarageDoorController.GarageDoorAction.Open);
            stateMachine.HandleInput(GarageDoorController.GarageDoorAction.Open);
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
        }
    }
}
