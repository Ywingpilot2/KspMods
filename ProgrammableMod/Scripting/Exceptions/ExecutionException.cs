using ActionLanguage.Exceptions;

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