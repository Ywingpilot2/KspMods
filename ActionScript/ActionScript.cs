using System;
using System.Collections.Generic;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Reflection;
using ActionLanguage.Token;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Functions.Single;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionLanguage
{
    public sealed class ActionScript : ITokenHolder
    {
        public ITokenHolder Container { get; }

        private Dictionary<string, IFunction> Functions { get; }
        private Dictionary<string, BaseTerm> Terms { get; }
        private Dictionary<string, object> _compiledValues;
        private LibraryManager LibraryManager { get; }
        private List<TokenCall> TokenCalls { get; }

        #region Tokens

        public int TotalTokens { get; set; }
        public int CallTokens { get; set; }
        public int KeyTokens { get; set; }
        public int TermTokens { get; set; }

        #endregion

        #region Enumeration

        public IEnumerable<TokenCall> EnumerateCalls() => TokenCalls;

        public IEnumerable<BaseTerm> EnumerateTerms() => Terms.Values;

        #endregion

        #region Functions

        public IFunction GetFunction(string name)
        {
            if (LibraryManager.HasFunction(name))
                return LibraryManager.GetFunction(name);
            
            if (!Functions.ContainsKey(name))
                throw new FunctionNotExistException(0, name);

            return Functions[name];
        }

        public bool HasFunction(string name) => Functions.ContainsKey(name) || LibraryManager.HasFunction(name);
        
        public void AddCall(TokenCall call)
        {
            TokenCalls.Add(call);
        }
        
        public void AddFunc(IFunction function)
        {
            if (Functions.ContainsKey(function.Name))
                throw new FunctionExistsException(0, function.Name);
            
            Functions.Add(function.Name, function);
        }

        #endregion

        #region Terms

        public BaseTerm GetTerm(string name)
        {
            if (LibraryManager.HasGlobalTerm(name))
                return LibraryManager.GetGlobalTerm(name);
            
            if (!Terms.ContainsKey(name))
                throw new TermNotExistException(0, name);

            return Terms[name];
        }

        public bool HasTerm(string name) => Terms.ContainsKey(name) || LibraryManager.HasGlobalTerm(name);

        public void AddTerm(BaseTerm term)
        {
            if (Terms.ContainsKey(term.Name))
                throw new TermAlreadyExistsException(0, term.Name);
            
            Terms.Add(term.Name, term);
        }

        public LibraryManager GetLibraryManager() => LibraryManager;

        #endregion

        #region Execution

        public int CurrentLine;
        public void Execute()
        {
            PreExecution();
            foreach (TokenCall functionCall in TokenCalls)
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
                catch (Exception e)
                {
                    throw new InvalidActionException(functionCall.Line, $"Internal error occured at {functionCall.Line}: \n{e.Message}\n{e.StackTrace}");
                }
#endif
            }
        }

        internal void PreExecution()
        {
            foreach (BaseTerm term in EnumerateTerms())
            {
                term.SetValue(_compiledValues[term.Name]);
            }
        }

        internal void PostCompilation()
        {
            foreach (TokenCall call in TokenCalls)
            {
                call.PostCompilation();
            }

            foreach (BaseTerm term in EnumerateTerms())
            {
                _compiledValues.Add(term.Name, term.GetValue());
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

        public ActionScript()
        {
            Functions = new Dictionary<string, IFunction>();
            Terms = new Dictionary<string, BaseTerm>();
            TokenCalls = new List<TokenCall>();
            LibraryManager = new LibraryManager();
            _compiledValues = new Dictionary<string, object>();
        }

        #endregion
    }
}