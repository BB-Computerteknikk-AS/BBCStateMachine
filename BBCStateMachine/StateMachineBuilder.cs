using System;

namespace no.bbc.StateMachine
{
    public class StateMachineBuilder<STATE_T, INPUT_T>
            where STATE_T : struct, IConvertible
            where INPUT_T : struct, IConvertible
    {
        #region Private Fields

        private StateMachine<STATE_T, INPUT_T> _machine;

        #endregion

        #region Public Constructor

        public StateMachineBuilder(StateMachine<STATE_T, INPUT_T> machine)
        {
            _machine = machine;
        }

        #endregion

        #region Internal Properties

        internal StateMachine<STATE_T, INPUT_T> Machine
        {
            get
            {
                return _machine;
            }
        }

        #endregion

        #region Public Methods

        public IfStateBuilder<STATE_T, INPUT_T> IfState(STATE_T state)
        {
            return new IfStateBuilder<STATE_T, INPUT_T>(this, state);
        }

        #endregion
    }

    public class IfStateBuilder<STATE_T, INPUT_T>
               where STATE_T : struct, IConvertible
               where INPUT_T : struct, IConvertible
    {
        #region Private Fields

        private StateMachineBuilder<STATE_T, INPUT_T> _stateMachineBuilder;
        private STATE_T _state;

        #endregion

        #region Public Constructor

        public IfStateBuilder(StateMachineBuilder<STATE_T, INPUT_T> stateMachineBuilder, STATE_T state)
        {
            _stateMachineBuilder = stateMachineBuilder;
            _state = state;
        }

        #endregion

        #region Internal Properties

        internal StateMachineBuilder<STATE_T, INPUT_T> StateMachineBuilder
        {
            get
            {
                return _stateMachineBuilder;
            }
        }

        internal STATE_T State
        {
            get
            {
                return _state;
            }
        }

        #endregion

        #region Public Methods

        public OnActionBuilder<STATE_T, INPUT_T> GotAction(INPUT_T action)
        {
            return new OnActionBuilder<STATE_T, INPUT_T>(this, action);
        }

        #endregion
    }

    public class OnActionBuilder<STATE_T, INPUT_T>
               where STATE_T : struct, IConvertible
               where INPUT_T : struct, IConvertible
    {
        #region Private Fields

        private IfStateBuilder<STATE_T, INPUT_T> _ifStateBuilder;
        private INPUT_T _action;

        #endregion

        #region Public Constructor

        public OnActionBuilder(IfStateBuilder<STATE_T, INPUT_T> ifStateBuilder, INPUT_T action)
        {
            _ifStateBuilder = ifStateBuilder;
            _action = action;
        }

        #endregion

        #region Internal Properties

        internal IfStateBuilder<STATE_T, INPUT_T> IfStateBuilder
        {
            get
            {
                return _ifStateBuilder;
            }
        }

        internal INPUT_T Action
        {
            get
            {
                return _action;
            }
        }

        #endregion

        #region Public Methods

        public TransitionToBuilder<STATE_T, INPUT_T> TransitionTo(STATE_T newState)
        {
            return new TransitionToBuilder<STATE_T, INPUT_T>(this, newState);
        }

        #endregion
    }

    public class TransitionToBuilder<STATE_T, INPUT_T>
           where STATE_T : struct, IConvertible
           where INPUT_T : struct, IConvertible
    {
        #region Private Fields

        private OnActionBuilder<STATE_T, INPUT_T> _onActionBuilder;
        private STATE_T _newState;
        private Action<StateMachine<STATE_T, INPUT_T>> _transitionAction;

        #endregion

        #region Internal Properties

        internal OnActionBuilder<STATE_T, INPUT_T> OnActionBuilder
        {
            get
            {
                return _onActionBuilder;
            }
        }

        internal STATE_T NewState
        {
            get
            {
                return _newState;
            }
        }

        #endregion

        #region Public Constructor

        public TransitionToBuilder(OnActionBuilder<STATE_T, INPUT_T> onActionBuilder, STATE_T newState)
        {
            _onActionBuilder = onActionBuilder;
            _newState = newState;
        }

        #endregion

        #region Public Methods

        public TransitionToBuilder<STATE_T, INPUT_T> Execute(Action<StateMachine<STATE_T, INPUT_T>> action)
        {
            _transitionAction = action;
            return this;
        }

        public StateMachineBuilder<STATE_T, INPUT_T> Build()
        {
            var machine = OnActionBuilder.IfStateBuilder.StateMachineBuilder.Machine;

            var state = OnActionBuilder.IfStateBuilder.State;
            var input = OnActionBuilder.Action;
            var output = NewState;

            machine.RegisterTransition(state, input, output);

            if (_transitionAction != null)
            {
                machine.OnTransition(state, input, _transitionAction);
            }
           
            return OnActionBuilder.IfStateBuilder.StateMachineBuilder;
        }

        #endregion
    }
}
