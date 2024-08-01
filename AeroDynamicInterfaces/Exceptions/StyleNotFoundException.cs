using System;

namespace AeroDynamicKerbalInterfaces.Exceptions;

public class StyleNotFoundException : Exception
{
    private readonly string? _style;

    public override string Message
    {
        get
        {
            if (_style != null)
                return $"Cannot find style called \"{_style}\" are you sure you spelt it correctly?";

            return "Style was set to null for a control, therefore it cannot be found";
        }
    }

    public StyleNotFoundException(string style)
    {
        _style = style;
    }
}