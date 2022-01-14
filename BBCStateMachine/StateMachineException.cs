using System;

namespace no.bbc.StateMachine
{
    public class StateMachineException : Exception
    {
        public enum StateMachineErrorCode
        {
            Unknown,
            TransitionNotImplemented,
            DuplicateTransitionHandler,
            DuplicateTransition
        }

        public StateMachineException(StateMachineErrorCode errorCode, string message = null) : base(message)
        {
            ErrorCode = errorCode;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public StateMachineErrorCode ErrorCode { get; private set; }
    }
}
