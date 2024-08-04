using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Exceptions;

public class ExecutionException : SteelException
{
    public override string Message => $"An action script has encountered an error at {LineNumber}!";

    public ExecutionException(int lineNumber) : base(lineNumber)
    {
    }
}

public class InvalidActionException : ExecutionException
{
    private readonly string _message;
    public override string Message => $"Invalid action conducted at {LineNumber}, {_message}";

    public InvalidActionException(int lineNumber, string message) : base(lineNumber)
    {
        _message = message;
    }
}

public class InvalidAssignmentException : ExecutionException
{
    private readonly string _name;
    private readonly string _type;
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

    public InvalidAssignmentException(int lineNumber, BaseTerm term) : this(lineNumber, term.Name, term.ValueType)
    {
    }

    public InvalidAssignmentException(int lineNumber, string name, string type) : base(lineNumber)
    {
        _name = name;
        _type = type;
    }
}

public class FunctionLacksReturnException : ExecutionException
{
    private readonly IFunction _function;

    public override string Message
    {
        get
        {
            if (_function != null)
            {
                return $"func {_function.Name} is set to return {_function.ReturnType} yet lacks a return, are you sure all execution paths return a value?";
            }
            else
            {
                return $"function is set to return a value yet lacks a return, are you sure all execution paths return a value?";
            }
        }
    }

    public FunctionLacksReturnException(int lineNumber, IFunction function) : base(lineNumber)
    {
        _function = function;
    }
}

public class UserException : ExecutionException
{
    private readonly string _message;
    public override string Message => _message;

    public UserException(int lineNumber, string message) : base(lineNumber)
    {
        _message = message;
    }
}