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
#if DEBUG
                    throw new InvalidActionException(functionCall.Line, $"Internal error occured at {functionCall.Line}: \n{e.Message}\n{e.StackTrace}");
#else
                    throw new InvalidActionException(functionCall.Line, $"Internal error occured at {functionCall.Line}: \n{e.Message}");
#endif
                }
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