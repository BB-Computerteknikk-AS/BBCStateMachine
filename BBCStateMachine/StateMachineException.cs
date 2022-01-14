using System;

namespace no.bbc.StateMachine
{
    public class StateMachineException : Exception
    {
        public enum StateMachineErrorCode
        {
            TransitionNotImplemented,
            Unknown
        }

        public StateMachineException(StateMachineErrorCode errorCode, string? message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public StateMachineErrorCode ErrorCode { get; private set; }
    }
}
