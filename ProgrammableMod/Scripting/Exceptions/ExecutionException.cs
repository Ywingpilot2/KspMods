using ProgrammableMod.Scripting.Config.ValueStasher;
using SteelLanguage.Exceptions;

namespace ProgrammableMod.Scripting.Exceptions;

internal class KerbnetLostException : ExecutionException
{
    public override string Message => $"Call made at line {LineNumber} cannot be made, kerb net connection has been lost!";

    public KerbnetLostException(int lineNumber) : base(lineNumber)
    {
    }
}

internal class ControlLostException : ExecutionException
{
    public override string Message => $"Call made at line {LineNumber} cannot be made, vessel is not currently controllable!";

    public ControlLostException(int lineNumber) : base(lineNumber)
    {
    }
}

internal class TooSlowException : ExecutionException
{
    public override string Message => "Our engineers typically suggest writing scripts which don't loop forever, so they have shut down the script to prevent such a time paradox.";

    public TooSlowException(int lineNumber) : base(lineNumber)
    {
    }
}

internal class TypeNotStashableException : ExecutionException
{
    private readonly string _type;
    public override string Message => $"The the type {_type} at line {LineNumber} does not support being stashed onto the kerbnet";

    public TypeNotStashableException(int lineNumber, string type) : base(lineNumber)
    {
        _type = type;
    }
}

internal class StashableNotFoundException : ExecutionException
{
    private readonly string _name;
    public override string Message => $"Call at {LineNumber} cannot be made as stashable of name {_name} does not exist";

    public StashableNotFoundException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}

internal class StashableInvalidException : ExecutionException
{
    private readonly ProtoStash _stash;

    public override string Message => $"Stashable {_stash.Name} of type {_stash.ValueType} requested at line {LineNumber} was invalid";

    public StashableInvalidException(int lineNumber, ProtoStash stash) : base(lineNumber)
    {
        _stash = stash;
    }
}

internal class PartNotFoundException : ExecutionException
{
    private readonly string _name;
    public override string Message => $"Could not find part named \"{_name}\" at line number {LineNumber}";

    public PartNotFoundException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}

internal class ActionNotFoundException : ExecutionException
{
    private readonly string _name;
    public override string Message => $"Could not find action named \"{_name}\" at line number {LineNumber}";
    
    public ActionNotFoundException(int lineNumber, string name) : base(lineNumber)
    {
        _name = name;
    }
}