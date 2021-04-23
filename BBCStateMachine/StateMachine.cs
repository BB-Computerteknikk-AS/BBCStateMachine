using System;
using System.Collections.Generic;
using System.Linq;

namespace no.bbc.StateMachine
{
    public class StateMachine<STATE_T, INPUT_T>
        where STATE_T : struct, IConvertible
        where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public StateMachine(STATE_T initialState, NLog.Logger logger = null)
        {
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                _logger = NLog.LogManager.GetLogger(nameof(StateMachine));
            }

            _logger.Info("Initializing");
            if (!typeof(STATE_T).IsEnum)
            {
                throw new ArgumentException("expected an enum, but got " + typeof(STATE_T).Name, nameof(STATE_T));
            }


            if (!typeof(INPUT_T).IsEnum)
            {
                throw new ArgumentException("expected an enum, but got " + typeof(INPUT_T).Name, nameof(INPUT_T));
            }


            CurrentState = initialState;
        }


        ~StateMachine()
        {
            _onEnterActions.Clear();
            _onEnterActions = null;

            _onTransitionActions.Clear();
            _onTransitionActions = null;
        }

        #endregion

        #region Private Fields

        private NLog.Logger _logger;
        private Dictionary<Tuple<STATE_T, INPUT_T>, STATE_T> _transitionTable = new();

        private Dictionary<STATE_T, OnStateEnterDelegate> _onEnterActions = new();
        private Dictionary<Tuple<STATE_T, INPUT_T>, Action> _onTransitionActions = new();
        private StateMachineBuilder<STATE_T, INPUT_T> _builder;

        #endregion

        #region Public Delegates

        public delegate void OnStateChangedDelegate(STATE_T prevState, STATE_T newState, INPUT_T action);

        #endregion

        #region Public Properties

        public STATE_T CurrentState { get; private set; }

        public event OnStateChangedDelegate OnStateChanged;

        public IReadOnlyDictionary<Tuple<STATE_T, INPUT_T>, STATE_T> TransitionTable
        {
            get
            {
                return _transitionTable;
            }
        }

        public StateMachineBuilder<STATE_T, INPUT_T> Builder
        {
            get
            {
                if(_builder == null)
                {
                    _builder = new StateMachineBuilder<STATE_T, INPUT_T>(this);
                }

                return _builder;
            }
        }

        #endregion

        #region Public Delegates

        public delegate void OnStateEnterDelegate(STATE_T prevState, STATE_T newState, INPUT_T action); 

        #endregion

        #region Public Methods

        public void RegisterTransition(STATE_T state, INPUT_T input, STATE_T output)
        {
            var transitionKey = Tuple.Create(state, input);

            if (_transitionTable.ContainsKey(transitionKey))
            {
                throw new ArgumentOutOfRangeException("Cannot register duplicate transitions");
            }

            _transitionTable[transitionKey] = output;

            _logger.Info($"Registered Transition: '{state}' + '{input}' = '{output}'");
        }

        public IEnumerable<StateMachineTransition<STATE_T, INPUT_T>> GetPossibleTransitions()
        {
            // state -> input -> output
            var possibleTransitions = new List<StateMachineTransition<STATE_T, INPUT_T>>();

            var availableStates = typeof(STATE_T).GetEnumValues();
            var availableInputs = typeof(INPUT_T).GetEnumValues();

            foreach (INPUT_T input in availableInputs)
            {
                foreach (STATE_T state in availableStates)
                {
                    foreach (STATE_T output in availableStates)
                    {
                        var transition = new StateMachineTransition<STATE_T, INPUT_T>(state, input, output);
                        possibleTransitions.Add(transition);
                    }
                }
            }

            var registeredTransitions = _transitionTable.Select(x =>
            {
                return Tuple.Create(x.Key.Item1, x.Key.Item2, x.Value);
            });

            return possibleTransitions;
        }

        public IEnumerable<StateMachineTransition<STATE_T, INPUT_T>> GetUnhandledTransitions()
        {
            var possibleTransitions = GetPossibleTransitions();

            var registeredTransitions = _transitionTable.Select(x =>
            {
                return new StateMachineTransition<STATE_T, INPUT_T>(x.Key.Item1, x.Key.Item2, x.Value);
            });

            var unhandledTransitions = possibleTransitions.Except(registeredTransitions);

            return unhandledTransitions;
        }

        public IEnumerable<INPUT_T> GetUnhandledInputs()
        {
            var availableInputs = typeof(INPUT_T).GetEnumValues();

            var unhandledInputs = new List<INPUT_T>();

            foreach (INPUT_T input in availableInputs)
            {
                var registeredInputTransitions = _transitionTable.Where(x =>
                {
                    return EqualityComparer<INPUT_T>.Default.Equals(x.Key.Item2, input);
                });

                if (registeredInputTransitions.Count() < 1)
                {
                    unhandledInputs.Add(input);
                }
            }

            return unhandledInputs;
        }

        public STATE_T HandleInput(INPUT_T input)
        {
            lock (this)
            {
                // we conbine the state and input, and check the transition table
                var key = Tuple.Create(CurrentState, input);

                if (!_transitionTable.ContainsKey(key))
                {
                    var error = $"No transition registered from state: '{CurrentState}' with input: '{input}'";
                    _logger.Error(error);
                    throw new ArgumentException(error);
                }

                var prevState = CurrentState;
                var newState = _transitionTable[key];

                CurrentState = newState;

                _logger.Info($"Transition - '{prevState}' + '{input}' = '{CurrentState}'");

                if (_onEnterActions.ContainsKey(newState))
                {
                    var enterAction = _onEnterActions[newState];
                    enterAction?.Invoke(prevState, newState, input);
                }

                if (_onTransitionActions.ContainsKey(key))
                {
                    var transitionAction = _onTransitionActions[key];
                    transitionAction?.Invoke();
                }

                OnStateChanged?.Invoke(prevState, newState, input);

                return CurrentState;
            }
        }

        /// <summary>
        /// Listen for a transition to a new state, ignoring the input
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        public void OnEnterState(STATE_T state, OnStateEnterDelegate action)
        {
            _onEnterActions[state] = action;
        }

        /// <summary>
        /// Listen for a transition to a new state with a specific input
        /// </summary>
        /// <param name="state"></param>
        /// <param name="input"></param>
        /// <param name="action"></param>
        public void OnTransition(STATE_T state, INPUT_T input, Action action)
        {
            var transitionKey = Tuple.Create(state, input);

            if (_onTransitionActions.ContainsKey(transitionKey))
            {
                throw new ArgumentOutOfRangeException("Cannot register duplicate transitions handlers");
            }

            _onTransitionActions[transitionKey] = action;
        }

        #endregion

        #region Public Static Methods


        #endregion
    }
}
