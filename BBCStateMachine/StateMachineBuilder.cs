﻿using System;

namespace no.bbc.StateMachine
{
    public class StateMachineBuilder<STATE_T, INPUT_T>
            where STATE_T : struct, IConvertible
            where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public StateMachineBuilder(StateMachine<STATE_T, INPUT_T> machine)
        {
            Machine = machine;
        }

        #endregion

        #region Internal Properties

        internal StateMachine<STATE_T, INPUT_T> Machine { get; private set; }

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
        #region Public Constructor

        public IfStateBuilder(StateMachineBuilder<STATE_T, INPUT_T> stateMachineBuilder, STATE_T state)
        {
            StateMachineBuilder = stateMachineBuilder;
            State = state;
        }

        #endregion

        #region Internal Properties

        internal StateMachineBuilder<STATE_T, INPUT_T> StateMachineBuilder { get; private set; }

        internal STATE_T State { get; private set; }

        #endregion

        #region Public Methods

        public OnInputBuilder<STATE_T, INPUT_T> GotInput(INPUT_T input)
        {
            return new OnInputBuilder<STATE_T, INPUT_T>(this, input);
        }

        public OnEnterBuilder<STATE_T, INPUT_T> OnEnter(StateMachine<STATE_T, INPUT_T>.OnStateDelegate action)
        {
            return new OnEnterBuilder<STATE_T, INPUT_T>(this, action);
        }

        #endregion
    }

    public class OnEnterBuilder<STATE_T, INPUT_T>
               where STATE_T : struct, IConvertible
               where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public OnEnterBuilder(IfStateBuilder<STATE_T, INPUT_T> ifStateBuilder, StateMachine<STATE_T, INPUT_T>.OnStateDelegate action)
        {
            IfStateBuilder = ifStateBuilder;
            Action = action;
        }

        #endregion

        #region Internal Properties

        internal IfStateBuilder<STATE_T, INPUT_T> IfStateBuilder { get; private set; }
        internal StateMachine<STATE_T, INPUT_T>.OnStateDelegate Action { get; private set; }

        #endregion

        #region Public Methods

        public void Build()
        {
            IfStateBuilder.StateMachineBuilder.Machine.SetOnEnterStateAction(IfStateBuilder.State, Action);
        }

        #endregion
    }

    public class OnInputBuilder<STATE_T, INPUT_T>
               where STATE_T : struct, IConvertible
               where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public OnInputBuilder(IfStateBuilder<STATE_T, INPUT_T> ifStateBuilder, INPUT_T input)
        {
            IfStateBuilder = ifStateBuilder;
            Input = input;
        }

        public OnInputBuilder(OnEnterBuilder<STATE_T, INPUT_T> onEnterBuilder, INPUT_T input)
        {
            IfStateBuilder = onEnterBuilder.IfStateBuilder;
            Input = input;
        }

        #endregion

        #region Internal Properties

        internal IfStateBuilder<STATE_T, INPUT_T> IfStateBuilder { get; private set; }

        internal INPUT_T Input { get; private set; }

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
        private StateMachine<STATE_T, INPUT_T>.OnStateDelegate _onTransition;

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
            _onTransition = null;
        }

        #endregion

        #region Public Methods

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

            if (_onTransition != null)
            {
                machine.SetOnTransitionAction(state, input, _onTransition);
            }

            return OnInputBuilder.IfStateBuilder.StateMachineBuilder;
        }

        #endregion
    }
}
