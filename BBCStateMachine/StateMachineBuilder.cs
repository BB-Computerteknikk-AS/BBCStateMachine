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

        public OnInputBuilder<STATE_T, INPUT_T> GotInput(INPUT_T input)
        {
            return new OnInputBuilder<STATE_T, INPUT_T>(this, input);
        }

        #endregion
    }

    public class OnInputBuilder<STATE_T, INPUT_T>
               where STATE_T : struct, IConvertible
               where INPUT_T : struct, IConvertible
    {
        #region Private Fields

        private IfStateBuilder<STATE_T, INPUT_T> _ifStateBuilder;
        private INPUT_T _input;

        #endregion

        #region Public Constructor

        public OnInputBuilder(IfStateBuilder<STATE_T, INPUT_T> ifStateBuilder, INPUT_T input)
        {
            _ifStateBuilder = ifStateBuilder;
            _input = input;
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

        internal INPUT_T Input
        {
            get
            {
                return _input;
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

        private OnInputBuilder<STATE_T, INPUT_T> _onInputBuilder;
        private STATE_T _newState;
        private StateMachine<STATE_T, INPUT_T>.OnStateDelegate _onEnter;
        private StateMachine<STATE_T, INPUT_T>.OnStateDelegate _onTransition;
        private StateMachine<STATE_T, INPUT_T>.OnStateDelegate _onExit;

        #endregion

        #region Internal Properties

        internal OnInputBuilder<STATE_T, INPUT_T> OnInputBuilder
        {
            get
            {
                return _onInputBuilder;
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

        public TransitionToBuilder(OnInputBuilder<STATE_T, INPUT_T> onInputBuilder, STATE_T newState)
        {
            _onInputBuilder = onInputBuilder;
            _newState = newState;
        }

        ~TransitionToBuilder()
        {
            _onExit = null;
            _onTransition = null;
            _onExit = null;
        }

        #endregion

        #region Public Methods

        public TransitionToBuilder<STATE_T, INPUT_T> OnEnter(StateMachine<STATE_T, INPUT_T>.OnStateDelegate action)
        {
            _onEnter = action;
            return this;
        }

        public TransitionToBuilder<STATE_T, INPUT_T> OnExit(StateMachine<STATE_T, INPUT_T>.OnStateDelegate action)
        {
            _onExit = action;
            return this;
        }

        public TransitionToBuilder<STATE_T, INPUT_T> OnTransition(StateMachine<STATE_T, INPUT_T>.OnStateDelegate action)
        {
            _onTransition = action;
            return this;
        }

        public StateMachineBuilder<STATE_T, INPUT_T> Build()
        {
            var machine = OnInputBuilder.IfStateBuilder.StateMachineBuilder.Machine;

            var state = OnInputBuilder.IfStateBuilder.State;
            var input = OnInputBuilder.Input;
            var output = NewState;

            machine.RegisterTransition(state, input, output);

            if (_onEnter != null)
            {
                machine.SetOnEnterStateAction(state, _onEnter);
            }

            if (_onTransition != null)
            {
                machine.SetOnTransitionAction(state, input, _onTransition);
            }

            if(_onExit != null)
            {
                machine.SetOnExitStateAction(state, _onExit);
            }

            return OnInputBuilder.IfStateBuilder.StateMachineBuilder;
        }

        #endregion
    }
}
