using System;
using System.Collections.Generic;

namespace ActionScript
{
    public struct FuncIn
    {
        public string Name { get; }
        public Term Input { get; set; }

        public FuncIn(string name)
        {
            Name = name;
            Input = new Term();
        }
    }

    public struct Function
    {
        public string Name { get; }
        public Action<List<FuncIn>> Action { get; }

        public void Execute(List<FuncIn> terms)
        {
            Action.Invoke(terms);
        }

        public Function(string name, Action<List<FuncIn>> action)
        {
            Name = name;
            Action = action;
        }
    }
}