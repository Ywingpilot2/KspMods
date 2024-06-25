using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ActionLanguage;
using ActionLanguage.Library;
using ActionLanguage.Reflection;
using ActionLanguage.Token.Fields;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;
using ActionLanguage.Token.Terms.Literal;

namespace ActionTest
{
    public class ProgramLibrary : ILibrary
    {
        public string Name => "program";

        public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
        {
            new Function("print", "void", inputTypes:"string", action: terms =>
            {
                Console.WriteLine(terms[0].CastToStr());
                return new ReturnValue();
            }),
            new Function("read", "string", terms =>
            {
                string read = Console.ReadLine();
                return new ReturnValue(read, "string");
            })
        };

        public IEnumerable<GlobalTerm> GlobalTerms => new[]
        {
            new GlobalTerm("program", "program")
        };
        
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }

        public ProgramLibrary(ActionLibrary baseLibrary)
        {
            TypeLibrary = new TypeLibrary();
            
            TypeLibrary.AddTermType(new TermType(new Program.ProgramTerm(), baseLibrary.TypeLibrary.GetTermType("term", 0)));
        }
    }
    
    internal static class Program
    {
        private static Dictionary<string, ActionScript> _compiled = new Dictionary<string, ActionScript>();
        private static ActionCompiler _compiler = new ActionCompiler(new ProgramLibrary(ActionCompiler.Library));
        
        public class ProgramTerm : BaseTerm
        {
            public override string ValueType => "program";
            private string _value;
            
            public override bool SetValue(object value)
            {
                _value = value.ToString();
                return true;
            }

            public override IEnumerable<TermField> GetFields()
            {
                yield return new TermField("value", "string", _value, true);
            }

            public override bool SetField(string name, object value)
            {
                _value = value.ToString();
                return true;
            }

            public override bool CopyFrom(BaseTerm term)
            {
                _value = term.CastToStr();
                return true;
            }

            public override object GetValue()
            {
                return _value;
            }
        }

        public static void Main(string[] args)
        {
            bool interaction = false;

            Stopwatch stopwatch = new Stopwatch();
            while (true)
            {
                Console.WriteLine("Execute Action:");
                string input = Console.ReadLine();
                stopwatch.Start();
                if (string.IsNullOrEmpty(input))
                    continue;

                string[] split = input.Split(new []{'='}, 2);
                switch (split[0].Trim(' ', '\t'))
                {
                    case "compileLocal":
                    {
                        Console.WriteLine("Compiling file...");
                        string global = $"{AppDomain.CurrentDomain.BaseDirectory}{split[1].Trim('"', ' ', '\t')}";
                        CompileScript(global);
                    } break;
                    case "execCompiled":
                    {
                        if (!_compiled.ContainsKey(split[1].Trim('"', ' ', '\t')))
                        {
                            Console.WriteLine("Specified script either does not exist or has not been compiled");
                            continue;
                        }

                        _compiled[split[1].Trim('"', ' ', '\t')].Execute();
                    } break;
                    case "execFile":
                    {
                        Console.WriteLine("Executing file...");
                        ExecuteScript(split[1].Trim('"', ' '));
                    } break;
                    case "execLocal":
                    {
                        Console.WriteLine("Executing file...");
                        ExecuteScript(GetGlobalPath(split[1].Trim('"', ' ')));
                    } break;
                    case "begin-interaction":
                    {
                        interaction = true;
                        Console.WriteLine("Beginning AS-Interactive...");
                    } break;
                    case "exit":
                    {
                        return;
                    }
                }
                
                stopwatch.Stop();
                Console.WriteLine($"Executed action in {stopwatch.Elapsed}");
                stopwatch.Reset();
            }
        }

        private static void ExecuteScript(string fullPath)
        {
            CompileScript(fullPath)?.Execute();
        }

        private static ActionScript CompileScript(string fullPath)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("Specified file does not exist!");
                return null;
            }
            
            StreamReader reader = new StreamReader(fullPath);
            ActionScript script = _compiler.Compile(reader);

            if (_compiled.ContainsKey(fileInfo.Name))
            {
                _compiled.Remove(fileInfo.Name);
            }
            _compiled.Add(fileInfo.Name, script);

            return script;
        }

        public static string GetGlobalPath(string relativePath) => $"{AppDomain.CurrentDomain.BaseDirectory}{relativePath}";
    }
}