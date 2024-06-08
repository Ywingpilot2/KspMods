using System;

namespace ActionScript.Exceptions;

public class ActionException : Exception
{
    public int LineNumber { get; set; }
    
    public ActionException(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}