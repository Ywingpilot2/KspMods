namespace SteelLanguage.Token;

/// <summary>
/// Base implementation of a compiler token
/// </summary>
public interface IToken
{
    /// <summary>
    /// The line this token belongs to. Used for Debugging purposes.
    /// </summary>
    int Line { get; set; }
}