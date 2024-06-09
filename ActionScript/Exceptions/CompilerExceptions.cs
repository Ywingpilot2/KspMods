using System;

namespace ActionScript.Exceptions;

public class CompilationException : ActionException
{
    public override string Message => $"An action script has encountered an error at {LineNumber}!";
        
    public CompilationException(int lineNumber) : base(lineNumber)
    {
        LineNumber = lineNumber;
    }
}

public class InvalidCompilationException : CompilationException
{
    private string _message;

    public override string Message => _message ?? $"An unknown error occured at {LineNumber}";

    public InvalidCompilationException(int lineNumber, string message) : base(lineNumber)
    {
        _message = message;
    }
}

public class InvalidTermCastException : CompilationException
{
    public override string Message
    {
        get
        {
            if (_from != null && _to != null)
            {
                return $"Cannot cast {_from} to {_to}, line number: {LineNumber}";
            }
            else
            {
                return $"Invalid cast at line {LineNumber}";
            }
        }
    }
    private string _from;
    private string _to;

    public InvalidTermCastException(int lineNumber) : base(lineNumber)
    {
    }

    public InvalidTermCastException(int lineNumber, string from, string to) : base(lineNumber)
    {
        _from = from;
        _to = to;
    }
}

public class InvalidParametersException : CompilationException
{
    private string[] _strings;

    public override string Message
    {
        get
        {
            if (_strings == null)
            {
                return $"The function call at {LineNumber} has invalid parameters.";
            }
            else
            {
                if (_strings.Length == 0)
                {
                    return $"The function call at {LineNumber} expects no parameters yet has inputs.";
                }
                else
                {
                    string error = $"The function call at {LineNumber} param conditions are not met.\nIt expects {_strings.Length} inputs of the following types:";
                    foreach (string s in _strings)
                    {
                        error += $"\n{s}";
                    }

                    return error;
                }
            }
        }
    }

    public InvalidParametersException(int lineNumber) : base(lineNumber)
    {
    }

    public InvalidParametersException(int lineNumber, string[] ins) : base(lineNumber)
    {
        _strings = ins;
    }
}

public class TypeNotExistException : CompilationException
{
    private string _typeName;

    public override string Message
    {
        get
        {
            if (_typeName != null)
            {
                return $"Type {_typeName} specified at line {LineNumber} does not exist or could not be found, are you sure the library was imported?";
            }
            else
            {
                return $"Specified type at {LineNumber} does not exist or could not be found, are you sure you specified a type?";
            }
        }
    }

    public TypeNotExistException(int lineNumber, string typeName) : base(lineNumber)
    {
        _typeName = typeName;
    }
}

public class TypeNotConstructableException : CompilationException
{
    private string _type;

    public override string Message
    {
        get
        {
            if (_type == null)
                return $"Term constructed at {LineNumber} is not a constructable type(e.g is abstract)";
            else
                return $"Term of type {_type} at {LineNumber} cannot be constructed(e.g is abstract)";
        }
    }

    public TypeNotConstructableException(int lineNumber, string type) : base(lineNumber)
    {
        _type = type;
    }
}

public class FunctionNotExistException : CompilationException
{
    public override string Message
    {
        get
        {
            if (_name != null)
            {
                return $"Function called at line {LineNumber} does not exist!";
            }
            else
            {
                return $"Function {_name} called at line {LineNumber} does not exist!";
            }
        }
    }
    private string _name;
    
    public FunctionNotExistException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}

public class TermAlreadyExistsException : CompilationException
{
    private string _name;

    public override string Message
    {
        get
        {
            if (_name == null)
            {
                return $"Term at line {LineNumber} already exists therefore cannot be declared, did you mean to re-assign it?";
            }
            else
            {
                return $"Term {_name} at line {LineNumber} already exists therefore cannot be declared, did you mean to re-assign it?";
            }
        }
    }

    public TermAlreadyExistsException(int lineNumber, string name) : base(lineNumber)
    {
    }
}

public class TermNotExistException : CompilationException
{
    public override string Message
    {
        get
        {
            if (_name != null)
            {
                return $"Term called at line {LineNumber} does not exist!";
            }
            else
            {
                return $"Term {_name} called at line {LineNumber} does not exist!";
            }
        }
    }
    private string _name;
    
    public TermNotExistException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}

public class FunctionReturnsVoidException : CompilationException
{
    private string _function;

    public override string Message
    {
        get
        {
            if (_function == null)
            {
                return $"Function called at line {LineNumber} returns void, meaning it can't be used for assignment.";
            }
            else
            {
                return $"{_function} called at line {LineNumber} returns void, meaning it can't be used for assignment.";
            }
        }
    }

    public FunctionReturnsVoidException(int lineNumber, string function) : base(lineNumber)
    {
        _function = function;
    }
}

public class FunctionParamsInvalidException : CompilationException
{
    private string _token;

    public override string Message
    {
        get
        {
            if (_token != null)
            {
                return $"Function {_token} at line {LineNumber} has invalid parameters";
            }
            else
            {
                return $"Function at {LineNumber} has invalid parameters";
            }
        }
    }

    public FunctionParamsInvalidException(int lineNumber, string token) : base(lineNumber)
    {
        _token = token;
    }
}

public class FunctionLacksEndException : CompilationException
{
    private string _token;

    public override string Message
    {
        get
        {
            if (_token != null)
            {
                return $"Function {_token} at line {LineNumber} does not have or has an invalid declared body, are you sure you added curly brackets to the end?";
            }
            else
            {
                return $"Function at line {LineNumber} does not have or has an invalid declared body, are you sure you added curly brackets to the end?";
            }
        }
    }

    public FunctionLacksEndException(int lineNumber, string token) : base(lineNumber)
    {
        _token = token;
    }
}

public class FunctionExistsException : CompilationException
{
    private string _name;

    public override string Message => $"Function {_name} has already been declared";

    public FunctionExistsException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}