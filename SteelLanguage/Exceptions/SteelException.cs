using System;

namespace SteelLanguage.Exceptions;

public class SteelException : Exception
{
    public int LineNumber { get; set; }
    
    public SteelException(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}