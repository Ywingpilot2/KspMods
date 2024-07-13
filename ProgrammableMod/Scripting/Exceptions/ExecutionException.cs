using SteelLanguage.Exceptions;

namespace ProgrammableMod.Scripting.Exceptions;

public class KerbnetLostException : ExecutionException
{
    public override string Message => $"Call made at line {LineNumber} cannot be made, kerb net connection has been lost!";

    public KerbnetLostException(int lineNumber) : base(lineNumber)
    {
    }
}

public class ControlLostException : ExecutionException
{
    public override string Message => $"Call made at line {LineNumber} cannot be made, vessel is not currently controllable!";

    public ControlLostException(int lineNumber) : base(lineNumber)
    {
    }
}

public class TooSlowException : ExecutionException
{
    public override string Message => "Our engineers typically suggest writing scripts which don't loop forever, so they have shut down the script to prevent such a time paradox.";

    public TooSlowException(int lineNumber) : base(lineNumber)
    {
    }
}

public class TypeNotStashableException : ExecutionException
{
    private string _type;
    public override string Message => $"The the type {_type} at line {LineNumber} does not support being stashed onto the kerbnet";

    public TypeNotStashableException(int lineNumber, string type) : base(lineNumber)
    {
        _type = type;
    }
}

public class StashableNotFoundException : ExecutionException
{
    private string _name;
    public override string Message => $"Call at {LineNumber} cannot be made as stashable of name {_name} does not exist";

    public StashableNotFoundException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}