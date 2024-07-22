using System.Collections.Generic;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Terms;

namespace SteelLanguage.Token;

public interface IFunctionHolder
{
    public IFunction GetFunction(string name);
    public bool HasFunction(string name);
    public void AddFunc(IFunction function);
}

public interface ICallHolder
{
    public void AddCall(TokenCall call);
    public IEnumerable<TokenCall> EnumerateCalls();
    public IEnumerable<BaseTerm> EnumerateTerms();
}

/// <summary>
/// Base implementation of an interface capable of containing tokens
///
/// <seealso cref="BaseTerm"/>
/// <seealso cref="LibraryManager"/>
/// <seealso cref="TokenCall"/>
/// </summary>
public interface ITokenHolder : IFunctionHolder, ICallHolder
{
    /// <summary>
    /// The <see cref="ITokenHolder"/> which contains this. Null if none.
    /// </summary>
    public ITokenHolder Container { get; }

    /// <summary>
    /// Gets a <see cref="BaseTerm"/> from this holder
    /// </summary>
    /// <param name="name">The name of the term to get</param>
    /// <returns>The requested <see cref="BaseTerm"/></returns>
    public BaseTerm GetTerm(string name);
    
    /// <summary>
    /// Whether or not this holder has a <see cref="BaseTerm"/> with the specified name
    /// </summary>
    /// <param name="name">The name of the term to find</param>
    /// <returns>A bool indicating whether this container has the specified term. True if found, false if not.</returns>
    public bool HasTerm(string name);
    
    /// <summary>
    /// Adds a <see cref="BaseTerm"/> to this holder
    /// </summary>
    /// <param name="term">The term to add</param>
    public void AddTerm(BaseTerm term);

    /// <summary>
    /// Gets the current <see cref="SteelScript"/>'s <see cref="LibraryManager"/>
    /// </summary>
    /// <returns>The <see cref="SteelScript"/>'s <see cref="LibraryManager"/></returns>
    /// <example>
    /// <code>
    /// public LibraryManager GetLibraryManager()
    /// {
    ///     return Container.GetLibraryManager();
    /// }
    /// </code>
    /// </example>
    public LibraryManager GetLibraryManager();
}