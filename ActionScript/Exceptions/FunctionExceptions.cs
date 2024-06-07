using System;

namespace ActionScript.Exceptions
{
    public class ActionException : Exception
    {
        public int LineNumber { get; set; }
        public override string Message => $"An action script has encountered an error at {LineNumber}!";

        public ActionException(int lineNumber)
        {
            LineNumber = lineNumber;
        }
    }

    public class InvalidParametersException : ActionException
    {
        public override string Message => $"The function at {LineNumber} has invalid parameters.";

        public InvalidParametersException(int lineNumber) : base(lineNumber)
        {
        }
    }
}