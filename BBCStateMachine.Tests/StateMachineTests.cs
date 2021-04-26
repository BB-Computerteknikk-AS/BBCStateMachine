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

            PrinterStateMachine = new StateMachine<PrinterState, PrinterInput>(PrinterState.Disconnected);
        }

        public enum PrinterState
        {
            Disconnected,
            Disconnecting,
            Connecting,
            WaitingForPrint,
            NotFound,
            PrintingData,
            PaperJammed,
        }

        public enum PrinterInput
        {
            Connect,
            WaitForPrint,
            NotFound,
            Disconnect,
            Disconnected,
            PrintData,
            PaperJammed
        }

        private StateMachine<PrinterState, PrinterInput> PrinterStateMachine { get; set; }
        private StateMachineGraphCompiler<PrinterState, PrinterInput> PrinterStateMachineGraphCompiler { get; set; }

        [Fact(DisplayName = "Test State Machine")]
        public void InitPrinterStateMachine()
        {
            var disconnectedEvent = new AutoResetEvent(false);

            // First we describe our state machine using a StateMachineBuilder, which is a fluent interface
            PrinterStateMachine = new StateMachine<PrinterState, PrinterInput>(PrinterState.Disconnected);

            PrinterStateMachine.Builder
                .IfState(PrinterState.Disconnected)
                .GotInput(PrinterInput.Connect)
                .TransitionTo(PrinterState.Connecting)               
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.Connecting)
                .GotInput(PrinterInput.WaitForPrint)
                .TransitionTo(PrinterState.WaitingForPrint)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.Connecting)
                .GotInput(PrinterInput.NotFound)
                .TransitionTo(PrinterState.Disconnected)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.WaitingForPrint)
                .GotInput(PrinterInput.PrintData)
                .TransitionTo(PrinterState.PrintingData)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PrintingData)
                .GotInput(PrinterInput.WaitForPrint)
                .TransitionTo(PrinterState.WaitingForPrint)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PrintingData)
                .GotInput(PrinterInput.PaperJammed)
                .TransitionTo(PrinterState.PaperJammed)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PaperJammed)
                .GotInput(PrinterInput.Disconnect)
                .TransitionTo(PrinterState.Disconnecting)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PaperJammed)
                .GotInput(PrinterInput.Connect)
                .TransitionTo(PrinterState.Connecting)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PaperJammed)
                .GotInput(PrinterInput.PrintData)
                .TransitionTo(PrinterState.PrintingData)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.WaitingForPrint)
                .GotInput(PrinterInput.Disconnect)
                .TransitionTo(PrinterState.Disconnecting)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PrintingData)
                .GotInput(PrinterInput.Disconnect)
                .TransitionTo(PrinterState.Disconnecting)
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.Disconnecting)
                .GotInput(PrinterInput.Disconnected)
                .TransitionTo(PrinterState.Disconnected)
                 .OnTransition((sender, prevState, newState, input) =>
                 {
                     // execute an action on this transition
                     disconnectedEvent.Set();
                 })
                .Build();

            try
            {
                // Attempt to trigger a transition that is not handled
                PrinterStateMachine.HandleInput(PrinterInput.PaperJammed);
                Assert.False(true);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // We expect to get an error because this is not a valid transition
                Assert.NotNull(ex);
            }

            // Handle the initial state change, starting our state machine
            PrinterStateMachine.HandleInput(PrinterInput.Connect);

            // The printer was not found
            PrinterStateMachine.HandleInput(PrinterInput.NotFound);

            Assert.True(PrinterStateMachine.CurrentState == PrinterState.Disconnected);

            // Try again
            PrinterStateMachine.HandleInput(PrinterInput.Connect);

            Assert.True(PrinterStateMachine.CurrentState == PrinterState.Connecting);

            PrinterStateMachine.HandleInput(PrinterInput.WaitForPrint);

            Assert.True(PrinterStateMachine.CurrentState == PrinterState.WaitingForPrint);

            // We're connected
            PrinterStateMachine.HandleInput(PrinterInput.PrintData);

            PrinterStateMachine.HandleInput(PrinterInput.PaperJammed);

            PrinterStateMachine.HandleInput(PrinterInput.PrintData);

            PrinterStateMachine.HandleInput(PrinterInput.Disconnect);

            PrinterStateMachine.HandleInput(PrinterInput.Disconnected);

            try
            {
                PrinterStateMachine.HandleInput(PrinterInput.PrintData);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Not possible, because the printer is disconnected
                Assert.NotNull(ex);
            }

            var graphCompiler = new StateMachineGraphCompiler<PrinterState, PrinterInput>(PrinterStateMachine);
            var graph = graphCompiler.CompileDotGraph();
            // results can be visualized here: http://graphviz.it

            Assert.NotEmpty(graph);

            disconnectedEvent.WaitOne();
        }
    }
}
