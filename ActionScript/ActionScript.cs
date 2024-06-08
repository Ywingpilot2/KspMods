using System;
using System.Collections.Generic;
using System.Linq;
using ActionScript.Exceptions;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript
{
    public class ActionScript
    {
        public Dictionary<string, Function> Functions { get; }
        public Dictionary<string, BaseTerm> Terms { get; }
        public List<TypeLibrary> TypeLibraries { get; }

        public List<TokenCall> TokenCalls { get; }

        #region Functions

        public Function GetFunction(string name)
        {
            if (!Functions.ContainsKey(name))
                throw new FunctionNotExistException(0, name);

            return Functions[name];
        }

        public bool HasFunction(string name) => Functions.ContainsKey(name);

        #endregion

        #region Terms

        public BaseTerm GetTerm(string name)
        {
            if (!Terms.ContainsKey(name))
                throw new FunctionNotExistException(0, name);

            return Terms[name];
        }

        public bool HasTerm(string name) => Terms.ContainsKey(name);

        #endregion

        #region Execution

        public int CurrentLine;
        public void Execute()
        {
            foreach (TokenCall functionCall in TokenCalls)
            {
                CurrentLine = functionCall.Line;
                try
                {
                    functionCall.Call();
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

        #endregion

        #region Type Library

        public bool TermTypeExists(string name)
        {
            foreach (TypeLibrary library in TypeLibraries)
            {
                if (library.HasTermType(name))
                    return true;
            }
            return false;
        }

        public TermType GetTermType(string name)
        {
            foreach (TypeLibrary library in TypeLibraries)
            {
                if (!library.HasTermType(name))
                    continue;

                return library.GetTermType(name, CurrentLine);
            }
            
            throw new TypeNotExistException(CurrentLine, name);
        }

        #endregion

        #region Constructors

        public ActionScript()
        {
            Functions = new Dictionary<string, Function>();
            Terms = new Dictionary<string, BaseTerm>();
            TokenCalls = new List<TokenCall>();
            TypeLibraries = new List<TypeLibrary>();
        }

        #endregion
    }
}