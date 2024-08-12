using System;
using System.Collections.Generic;
using SteelLanguage.Exceptions;
using SteelLanguage.Library;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Functions.Single;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Token.Terms;

namespace SteelLanguage;

/// <summary>
/// Represents a fully compiled and executable script
/// </summary>
/// <seealso cref="SteelCompiler"/>
public sealed class SteelScript : ITokenHolder
{
    public ITokenHolder Container { get; }

    private readonly Dictionary<string, IFunction> _functions;
    private Dictionary<string, TermHolder> _terms;
    private LibraryManager LibraryManager { get; }
    private readonly List<TokenCall> _tokenCalls;

    #region Tokens

    public int TotalTokens { get; set; }
    public int CallTokens { get; set; }
    public int KeyTokens { get; set; }
    public int TermTokens { get; set; }

    #endregion

    #region Enumeration

    public IEnumerable<TokenCall> EnumerateCalls() => _tokenCalls;

    public IEnumerable<BaseTerm> EnumerateTerms()
    {
        foreach (TermHolder holder in _terms.Values)
        {
            yield return holder.GetTerm();
        }
    }

    #endregion

    #region Functions

    public IFunction GetFunction(string name)
    {
        if (LibraryManager.HasFunction(name))
            return LibraryManager.GetFunction(name);
            
        if (!_functions.ContainsKey(name))
            throw new FunctionNotExistException(0, name);

        return _functions[name];
    }

    public bool HasFunction(string name) => _functions.ContainsKey(name) || LibraryManager.HasFunction(name);
        
    public void AddCall(TokenCall call)
    {
        _tokenCalls.Add(call);
    }
        
    public void AddFunc(IFunction function)
    {
        if (_functions.ContainsKey(function.Name))
            throw new FunctionExistsException(0, function.Name);
            
        _functions.Add(function.Name, function);
    }

    #endregion

    #region Terms

    public BaseTerm GetTerm(string name)
    {
        if (LibraryManager.HasGlobalTerm(name))
            return LibraryManager.GetGlobalTerm(name);
            
        if (!_terms.ContainsKey(name))
            throw new TermNotExistException(0, name);

        return _terms[name].GetTerm();
    }

    public TermHolder GetHolder(string name)
    {
        if (LibraryManager.HasGlobalTerm(name))
            return LibraryManager.GetGlobalHolder(name);
        
        if (!_terms.ContainsKey(name))
            throw new TermNotExistException(0, name);

        return _terms[name];
    }

    public bool HasTerm(string name) => _terms.ContainsKey(name) || LibraryManager.HasGlobalTerm(name);

    public void AddTerm(BaseTerm term)
    {
        if (_terms.ContainsKey(term.Name))
            throw new TermAlreadyExistsException(0, term.Name);

        TermHolder holder = new TermHolder(term.GetTermType().Name)
        {
            Name = term.Name
        };
        holder.SetTerm(term);
        
        _terms.Add(term.Name, holder);
    }

    public void AddHolder(TermHolder holder)
    {
        if (_terms.ContainsKey(holder.Name))
            throw new TermAlreadyExistsException(0, holder.Name);
        
        _terms.Add(holder.Name, holder);
    }

    public LibraryManager GetLibraryManager() => LibraryManager;

    #endregion

    #region Execution

    public int CurrentLine;
    public void Execute()
    {
        PreExecution();
        foreach (TokenCall functionCall in _tokenCalls)
        {
            CurrentLine = functionCall.Line;
            try
            {
                functionCall.PreExecution();
                ReturnValue value = functionCall.Call();
                functionCall.PostExecution();

                if (value.Value is ReturnCall)
                {
                    return;
                }
            }
#if DEBUG
            finally{} // I am too lazy to remove the try catch entirely when on debug
#else
            catch (ActionException e)
            {
                if (e.LineNumber == 0)
                {
                    e.LineNumber = functionCall.Line;
                }
                throw;
            }
            catch (AggregateException e)
            {
                if (e.InnerException != null && e.InnerExceptions.Count == 1)
                    throw new InvalidActionException(CurrentLine, e.InnerException.Message);

                string errors = "\n";
                foreach (Exception innerException in e.InnerExceptions)
                {
                    errors += innerException.Message;
                }

                throw new InvalidActionException(CurrentLine, errors);
            }
            catch (Exception e)
            {
                throw new InvalidActionException(functionCall.Line, e.Message);
            }
#endif
        }
        PostExecution();
    }

    private void PreExecution()
    {
    }

    private void PostExecution()
    {
    }

    internal void PostCompilation()
    {
        foreach (TokenCall call in _tokenCalls)
        {
            call.PostCompilation();
        }
    }

    #endregion

    #region Type Library
        
    [Obsolete]
    public bool TermTypeExists(string name) => LibraryManager.HasTermType(name);

    [Obsolete]
    public TermType GetTermType(string name) => LibraryManager.GetTermType(name);
        
    #endregion

    #region Keywords

    [Obsolete]
    public bool HasKeyword(string name) => LibraryManager.HasKeyword(name);

    [Obsolete]
    public IKeyword GetKeyword(string name) => LibraryManager.GetKeyword(name);

    #endregion

    #region Constructors

    public SteelScript()
    {
        _functions = new Dictionary<string, IFunction>();
        _terms = new Dictionary<string, TermHolder>();
        _tokenCalls = new List<TokenCall>();
        LibraryManager = new LibraryManager();
        new Dictionary<string, object>();
    }

    #endregion
}