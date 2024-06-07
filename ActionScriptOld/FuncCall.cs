using System;
using System.Collections.Generic;

namespace ActionScript
{
    public abstract class Call
    {
        public List<string> Terms { get; }
        protected ActionScript Script;

        public abstract void ExecuteCall();

        public Call(ActionScript script)
        {
            Script = script;
            Terms = new List<string>();
        }
        public Call(ActionScript script, IEnumerable<string> terms)
        {
            Script = script;
            Terms = new List<string>(terms);
        }
    }

    public sealed class FunctionCall : Call
    {
        public Function Function { get; }
        public List<FuncIn> Inputs { get; }

        public override void ExecuteCall()
        {
            for (var i = 0; i < Terms.Count; i++)
            {
                var name = Terms[i];
                if (!Script.ExecutingTerms.ContainsKey(name))
                    throw new InvalidOperationException($"Term {name} does not exist!");

                Term term = Script.ExecutingTerms[name];
                FuncIn @in = Inputs[i];
                @in.Input = term;
            }
            
            Function.Execute(Inputs);
        }
        
        public FunctionCall(ActionScript script, Function function, List<FuncIn> inputs) : base(script)
        {
            Function = function;
            Inputs = inputs;
        }

        public FunctionCall(ActionScript script, IEnumerable<string> terms, Function function, List<FuncIn> inputs) : base(script, terms)
        {
            Function = function;
            Inputs = inputs;
        }
    }
    
    public sealed class ConstructorCall : Call
    {
        private Term _term;
        
        public override void ExecuteCall()
        {
            if (Script.ExecutingTerms.ContainsKey(_term.Name))
            {
                Script.ExecutingTerms[_term.Name].CopyFrom(_term);
            }
            else
            {
                Script.ExecutingTerms.Add(_term.Name, _term);
            }
        }
        
        public ConstructorCall(ActionScript script, Term term) : base(script)
        {
            _term = term;
        }

        public ConstructorCall(ActionScript script, IEnumerable<string> terms, Term term) : base(script, terms)
        {
            _term = term;
        }
    }
}