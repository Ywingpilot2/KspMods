using System;

namespace ActionScript.Exceptions;

public class CompilationException : Exception
{
    public int LineNumber { get; set; }
    public override string Message => $"An action script has encountered an error at {LineNumber}!";
        
    public CompilationException(int lineNumber)
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