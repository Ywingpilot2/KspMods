using System;
using ActionScript.Token.Terms;

namespace ActionScript.Exceptions
{
    public class ExecutionException : ActionException
    {
        public override string Message => $"An action script has encountered an error at {LineNumber}!";

        public ExecutionException(int lineNumber) : base(lineNumber)
        {
        }
    }

    public class InvalidActionException : ExecutionException
    {
        private string _message;
        public override string Message => _message ?? $"An action script has encountered an error at {LineNumber}!";

        public InvalidActionException(int lineNumber, string message) : base(lineNumber)
        {
            _message = message;
        }
    }

    public class InvalidAssignmentException : ExecutionException
    {
        private string _name;
        private string _type;
        public override string Message
        {
            get
            {
                if (_name != null && _type != null)
                {
                    return $"Assignment of {_name}(type of {_type}) failed at line {LineNumber}";
                }
                else
                {
                    return $"Invalid cast at line {LineNumber}";
                }
            }
        }
        
        public InvalidAssignmentException(int lineNumber) : base(lineNumber)
        {
        }
        
        public InvalidAssignmentException(int lineNumber, BaseTerm term) : base(lineNumber)
        {
            if (term.Kind == TermKind.Null)
                return;
            
            _name = term.Name;
            _type = term.ValueType;
        }
    }
}