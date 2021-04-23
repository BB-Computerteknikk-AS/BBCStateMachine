using System;
namespace no.bbc.StateMachine
{
    public struct StateMachineTransition<STATE_T, INPUT_T>
        where STATE_T : struct, IConvertible
        where INPUT_T : struct, IConvertible
    {
        #region Public Constructor

        public StateMachineTransition(STATE_T state, INPUT_T input, STATE_T output)
        {
            if (!typeof(STATE_T).IsEnum)
            {
                throw new ArgumentException("expected an enum, but got " + typeof(STATE_T).Name, nameof(STATE_T));
            }


            if (!typeof(INPUT_T).IsEnum)
            {
                throw new ArgumentException("expected an enum, but got " + typeof(INPUT_T).Name, nameof(INPUT_T));
            }

            State = state;
            Input = input;
            Output = output;
        }

        #endregion

        #region Public Properties

        public STATE_T State { get; }
        public INPUT_T Input { get; }
        public STATE_T Output { get; }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return $"State: '{State}' Input: '{Input}' Output: '{Output}'";
        }

        #endregion
    }
}
