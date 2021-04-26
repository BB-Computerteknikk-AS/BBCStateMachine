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
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            // First we describe our state machine using a StateMachineBuilder, which is a fluent interface
            PrinterStateMachine = new StateMachine<PrinterState, PrinterInput>(PrinterState.Disconnected);

            string printedDocument = "";
            var hasSimulatedPaperJam = false;

            PrinterStateMachine.Builder
                .IfState(PrinterState.Disconnected)
                .GotInput(PrinterInput.Connect)
                .TransitionTo(PrinterState.Connecting)
                .OnTransition((sender, prevState, newState, input) =>
                {
                    // simulate work, then transition to WaitForPrint
                    Thread.Sleep(1000);
                    sender.HandleInput(PrinterInput.WaitForPrint);
                })
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.Connecting)
                .GotInput(PrinterInput.WaitForPrint)
                .TransitionTo(PrinterState.WaitingForPrint)
                .OnTransition((sender, prevState, newState, input) =>
                {
                    // simulate work
                    Thread.Sleep(1000);
                    sender.HandleInput(PrinterInput.PrintData);
                })
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.WaitingForPrint)
                .GotInput(PrinterInput.PrintData)
                .TransitionTo(PrinterState.PrintingData)
                .OnTransition((sender, prevState, newState, input) =>
                {
                    // simulate work
                    var print = "PRINT";
                    Console.WriteLine("Printing " + print);
                    Thread.Sleep(1000);

                    printedDocument += print;
                    sender.HandleInput(PrinterInput.WaitForPrint);

                    if (!hasSimulatedPaperJam)
                    {
                        Thread.Sleep(500);
                        sender.HandleInput(PrinterInput.PrintData);
                        Thread.Sleep(500);
                        sender.HandleInput(PrinterInput.PaperJammed);
                    }
                })
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PrintingData)
                .GotInput(PrinterInput.WaitForPrint)
                .TransitionTo(PrinterState.WaitingForPrint)
                .OnTransition((sender, prevState, newState, input) =>
                {
                    Console.WriteLine("Finished printing, waiting for another print");
                })
                .Build();

            PrinterStateMachine.Builder
                .IfState(PrinterState.PrintingData)
                .GotInput(PrinterInput.PaperJammed)
                .TransitionTo(PrinterState.PaperJammed)
                .OnTransition((sender, prevState, newState, input) =>
                {

                })
                .Build();

            try
            {
                // attempt to trigger a transition that is not handled
                PrinterStateMachine.HandleInput(PrinterInput.PaperJammed);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // we expect to get an error
                Assert.NotNull(ex);
            }

            // Handle the initial state change, starting our state machine
            PrinterStateMachine.HandleInput(PrinterInput.Connect);

            // autoResetEvent.Set();
            autoResetEvent.WaitOne(10000);
        }
    }
}
