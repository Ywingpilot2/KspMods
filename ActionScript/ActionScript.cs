using System;
using System.Collections.Generic;
using System.Linq;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript
{
    public class ActionScript
    {
        public Dictionary<string, Function> Functions { get; }
        public Dictionary<string, Term> Terms { get; }
        
        public List<TokenCall> TokenCalls { get; }

        #region Execution

        public void Execute()
        {
            foreach (TokenCall functionCall in TokenCalls)
            {
                functionCall.Call();
            }
        }

        #endregion

        #region Constructors

        public ActionScript(params ILibrary[] libraries)
        {
            Functions = new Dictionary<string, Function>();
            Terms = new Dictionary<string, Term>();
            TokenCalls = new List<TokenCall>();
        }

        #endregion
    }
}