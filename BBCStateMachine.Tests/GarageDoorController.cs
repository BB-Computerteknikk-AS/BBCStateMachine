using System;
using System.Threading.Tasks;

namespace no.bbc.StateMachine
{
    public class GarageDoorController
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

            var builder = _stateMachine.Builder;

            builder
                .IfState(GarageDoorState.Opening)
                .GotAction(GarageDoorAction.Open)
                .TransitionTo(GarageDoorState.Stopped)
                .Build();

            builder
                .IfState(GarageDoorState.Opening)
                .GotAction(GarageDoorAction.Close)
                .TransitionTo(GarageDoorState.Closing)
                .Execute(OnStartedClosing)
                .Build();

            builder
                .IfState(GarageDoorState.Opening)
                .GotAction(GarageDoorAction.OnOpened)
                .TransitionTo(GarageDoorState.Opened)
                .Execute(OnOpened)
                .Build();

            builder
                 .IfState(GarageDoorState.Opened)
                 .GotAction(GarageDoorAction.Open)
                 .TransitionTo(GarageDoorState.Opened)
                 .Execute(OnOpened)
                 .Build();

            builder
                 .IfState(GarageDoorState.Opened)
                 .GotAction(GarageDoorAction.Close)
                 .TransitionTo(GarageDoorState.Closing)
                 .Execute(OnStartedClosing)
                 .Build();

            builder
                 .IfState(GarageDoorState.Closing)
                 .GotAction(GarageDoorAction.Close)
                 .TransitionTo(GarageDoorState.Stopped)
                 .Build();

            builder
                 .IfState(GarageDoorState.Closing)
                 .GotAction(GarageDoorAction.Open)
                 .TransitionTo(GarageDoorState.Opening)
                 .Execute(OnStartedOpening)
                 .Build();

            builder
                 .IfState(GarageDoorState.Closing)
                 .GotAction(GarageDoorAction.OnClosed)
                 .TransitionTo(GarageDoorState.Closed)
                 .Execute(OnClosed)
                 .Build();

            builder
                 .IfState(GarageDoorState.Closed)
                 .GotAction(GarageDoorAction.Open)
                 .TransitionTo(GarageDoorState.Opening)
                 .Execute(OnStartedOpening)
                 .Build();

            builder
                 .IfState(GarageDoorState.Closed)
                 .GotAction(GarageDoorAction.Close)
                 .TransitionTo(GarageDoorState.Closed)
                 .Execute(OnClosed)
                 .Build();

            builder
                 .IfState(GarageDoorState.Stopped)
                 .GotAction(GarageDoorAction.Open)
                 .TransitionTo(GarageDoorState.Opening)
                 .Execute(OnStartedOpening)
                 .Build();

            builder
                 .IfState(GarageDoorState.Stopped)
                 .GotAction(GarageDoorAction.Close)
                 .TransitionTo(GarageDoorState.Closing)
                 .Execute(OnStartedClosing)
                 .Build();
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

        private void OnStartedOpening(StateMachine<GarageDoorState, GarageDoorAction> sender)
        {
            // simulate time consuiming work
            Task.Delay(1000).ContinueWith((t) =>
            {
                sender.HandleInput(GarageDoorAction.OnOpened);
            });
        }

        private void OnStartedClosing(StateMachine<GarageDoorState, GarageDoorAction> sender)
        {
            // simulate time consuiming work
            Task.Delay(1000).ContinueWith((t) =>
            {
                sender.HandleInput(GarageDoorAction.OnClosed);
            });
        }

        private void OnOpened(StateMachine<GarageDoorState, GarageDoorAction> sender)
        {
            OnDoorsOpened?.Invoke();
        }

        private void OnClosed(StateMachine<GarageDoorState, GarageDoorAction> sender)
        {
            OnDoorsClosed?.Invoke();
        }

        #endregion
    }
}
