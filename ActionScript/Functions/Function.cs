using System;
using ActionScript.Terms;

namespace ActionScript.Functions
{
    public struct ReturnValue
    {
        public bool HasValue => Value != null;
        public object Value { get; set; }

        public ReturnValue(object value)
        {
            Value = value;
        }
    }
    
    public struct Function
    {
        public string Name { get; }
        public string ValueType { get; set; }
        public Func<Term[], ReturnValue> Action { get; }

        public ReturnValue ExecuteAction(params Term[] terms)
        {
            return Action.Invoke(terms);
        }

        public Function(string name, Func<Term[], ReturnValue> action, string valueType = null)
        {
            Name = name;
            ValueType = valueType;
            Action = action;
        }
    }
}