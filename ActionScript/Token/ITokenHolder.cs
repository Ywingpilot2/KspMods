﻿using System.Collections.Generic;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token;

public interface IFunctionHolder
{
    public IFunction GetFunction(string name);
    public bool HasFunction(string name);
    public void AddFunc(IFunction function);
}

/// <summary>
/// Base implementation of an interface capable of handling tokens and their types
/// </summary>
public interface ITokenHolder : IFunctionHolder
{
    public ITokenHolder Container { get; }

    public IEnumerable<TokenCall> EnumerateCalls();
    public IEnumerable<BaseTerm> EnumerateTerms();
    
    public BaseTerm GetTerm(string name);
    public bool HasTerm(string name);
    
    public void AddCall(TokenCall call);
    public void AddTerm(BaseTerm term);

    public LibraryManager GetLibraryManager();
}