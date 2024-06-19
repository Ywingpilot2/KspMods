using System;
using System.Collections.Generic;
using System.Linq;
using ActionLanguage.Exceptions;
using ActionLanguage.Library;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionLanguage.Token.Functions
{
    public interface IExecutable
    {
        public ReturnValue Execute(params BaseTerm[] terms);

        public void PreExecution();
        public void PostExecution();
        public void PostCompilation();
    }

    public interface IFunction : IExecutable
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] InputTypes { get; }
        public bool AnyCount { get; } // TODO: actually implement optional params
        
        // TODO: this is a cool idea, why aren't we using it?
        // public bool InputIsValid(string type, int idx, ITokenHolder holder);
    }
    
    public struct Function : IFunction
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] InputTypes { get; }
        public bool AnyCount { get; }
        private Func<BaseTerm[], ReturnValue> Action { get; }

        public ReturnValue Execute(params BaseTerm[] terms)
        {
            return Action.Invoke(terms);
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        public void PostCompilation()
        {
        }

        public Function(string name, string returnType, Func<BaseTerm[], ReturnValue> action, params string[] inputTypes)
        {
            Name = name;
            ReturnType = returnType;
            InputTypes = inputTypes;
            Action = action;
        }
        
        public Function(string name, string returnType, Func<BaseTerm[], ReturnValue> action, bool anyCount, params string[] inputTypes)
        {
            Name = name;
            ReturnType = returnType;
            InputTypes = inputTypes;
            AnyCount = anyCount;
            Action = action;
        }
    }
    
    public class UserFunction : BaseExecutable, IFunction
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] InputTypes { get; }
        public bool AnyCount { get; }
        private readonly string[] _inputNames;

        public override ReturnValue Execute(params BaseTerm[] terms)
        {
            for (int i = 0; i < _inputNames.Length; i++)
            {
                string name = _inputNames[i];
                BaseTerm term = GetTerm(name);
                term.CopyFrom(terms[i]);
            }

            foreach (TokenCall call in Calls)
            {
                if (call is ReturnCall)
                    return call.Call();
                
                ReturnValue returnValue = call.Call();

                if (returnValue.HasValue)
                {
                    if (returnValue.Value is ReturnCall returnCall)
                        return returnCall.Call();
                }
            }

            if (ReturnType == "void")
            {
                return new ReturnValue();
            }
            else
                throw new FunctionLacksReturnException(0, this);
        }

        public UserFunction(ITokenHolder holder, string name, string returnType, Dictionary<string,string> inputs) : base(holder)
        {
            Name = name;
            ReturnType = returnType;
            InputTypes = new string[inputs.Count];
            _inputNames = new string[inputs.Count];

            int idx = 0;
            foreach (KeyValuePair<string,string> input in inputs)
            {
                InputTypes.SetValue(input.Value, idx);
                _inputNames.SetValue(input.Key, idx);

                LibraryManager manager = GetLibraryManager();
                AddTerm(manager.GetTermType(input.Value).Construct(input.Key, 0, manager));
                idx++;
            }
        }

        public override IEnumerable<BaseTerm> EnumerateTerms()
        {
            foreach (BaseTerm term in BaseTerms.Values)
            {
                yield return term;
            }
        }

        public override BaseTerm GetTerm(string name)
        {
            return BaseTerms[name];
        }

        public override bool HasTerm(string name)
        {
            return BaseTerms.ContainsKey(name);
        }
    }
}