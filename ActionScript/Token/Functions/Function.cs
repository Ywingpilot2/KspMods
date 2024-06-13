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
    }
    
    public interface IFunction : IExecutable
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] InputTypes { get; }
        public bool AnyCount { get; }
        
        public bool InputIsValid(string type, int idx, ITokenHolder holder);
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

        public bool InputIsValid(string type, int idx, ITokenHolder holder)
        {
            if (InputTypes.Length == 0)
                return false;
            
            if (AnyCount)
            {
                if (idx >= InputTypes.Length)
                {
                    string last = InputTypes.Last();
                    if (last != type)
                    {
                        TermType termType = holder.GetTermType(last);
                        return termType.IsSubclassOf(type);
                    }

                    return true;
                }
                else
                {
                    string at = InputTypes.ElementAtOrDefault(idx);
                    if (string.IsNullOrEmpty(at))
                        return false;

                    if (at != type)
                    {
                        TermType termType = holder.GetTermType(at);
                        return termType.IsSubclassOf(type);
                    }

                    return true;
                }
            }
            else
            {
                string at = InputTypes.ElementAtOrDefault(idx);
                if (string.IsNullOrEmpty(at))
                    return false;

                if (at != type)
                {
                    TermType termType = holder.GetTermType(at);
                    return termType.IsSubclassOf(type);
                }

                return true;
            }
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
    
    public class UserFunction : IFunction, ITokenHolder
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] InputTypes { get; }
        public bool AnyCount { get; }
        
        public ITokenHolder Container { get; }

        private Dictionary<string, BaseTerm> _baseTerms;
        private List<TokenCall> _calls;
        private ReturnCall _return;
        private readonly string[] _inputNames;

        private ActionScript _script;

        public ReturnValue Execute(params BaseTerm[] terms)
        {
            for (int i = 0; i < _inputNames.Length; i++)
            {
                string name = _inputNames[i];
                BaseTerm term = GetTerm(name);
                term.CopyFrom(terms[i]);
            }

            foreach (TokenCall call in _calls)
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

        public bool InputIsValid(string type, int idx, ITokenHolder holder)
        {
            if (InputTypes.Length == 0)
                return false;
            
            if (AnyCount)
            {
                if (idx >= InputTypes.Length)
                {
                    string last = InputTypes.Last();
                    if (last != type)
                    {
                        TermType termType = holder.GetTermType(last);
                        return termType.IsSubclassOf(type);
                    }

                    return true;
                }
                else
                {
                    string at = InputTypes.ElementAtOrDefault(idx);
                    if (string.IsNullOrEmpty(at))
                        return false;

                    if (at != type)
                    {
                        TermType termType = holder.GetTermType(at);
                        return termType.IsSubclassOf(type);
                    }

                    return true;
                }
            }
            else
            {
                string at = InputTypes.ElementAtOrDefault(idx);
                if (string.IsNullOrEmpty(at))
                    return false;

                if (at != type)
                {
                    TermType termType = holder.GetTermType(at);
                    return termType.IsSubclassOf(type);
                }

                return true;
            }
        }

        public UserFunction(string name, string returnType, Dictionary<string, string> inputs, ActionScript script)
        {
            Name = name;
            ReturnType = returnType;
            InputTypes = new string[inputs.Count];
            _inputNames = new string[inputs.Count];
            _baseTerms = new Dictionary<string, BaseTerm>();

            int idx = 0;
            foreach (KeyValuePair<string,string> input in inputs)
            {
                InputTypes.SetValue(input.Value, idx);
                _inputNames.SetValue(input.Key, idx);
                _baseTerms.Add(input.Key, script.GetTermType(input.Value).Construct(input.Key, 0)); // TODO: Set line
                idx++;
            }

            _script = script;
            _calls = new List<TokenCall>();
            Container = script;
        }
        
        public IEnumerable<TokenCall> EnumerateCalls()
        {
            foreach (TokenCall call in Container.EnumerateCalls())
            {
                yield return call;
            }

            foreach (TokenCall call in _calls)
            {
                yield return call;
            }
        }

        public IEnumerable<BaseTerm> EnumerateTerms()
        {
            foreach (BaseTerm term in _baseTerms.Values)
            {
                yield return term;
            }
        }

        public IFunction GetFunction(string name) => _script.GetFunction(name);

        public bool HasFunction(string name) => _script.HasFunction(name);

        public BaseTerm GetTerm(string name)
        {
            if (!_baseTerms.ContainsKey(name))
                throw new TermNotExistException(0, name);

            return _baseTerms[name];
        }

        public bool HasTerm(string name)
        {
            return _baseTerms.ContainsKey(name);
        }

        public void AddCall(TokenCall call)
        {
            _calls.Add(call);
        }

        public void AddTerm(BaseTerm term)
        {
            _baseTerms.Add(term.Name, term);
        }

        public void AddFunc(IFunction function)
        {
            throw new InvalidCompilationException(0, "Cannot declare a function within a function");
        }

        public bool TermTypeExists(string name) => _script.TermTypeExists(name);

        public TermType GetTermType(string name) => _script.GetTermType(name);
        public bool HasKeyword(string name) => _script.HasKeyword(name);

        public IKeyword GetKeyword(string name) => _script.GetKeyword(name);
    }
}