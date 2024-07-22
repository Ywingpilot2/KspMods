using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SteelLanguage;
using SteelLanguage.Library;
using SteelLanguage.Library.System;
using SteelLanguage.Library.System.Terms.Complex;
using SteelLanguage.Library.System.Terms.Complex.Enumerators;
using SteelLanguage.Library.System.Terms.Literal;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Fields;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.Interaction;
using SteelLanguage.Token.KeyWords;
using SteelLanguage.Token.KeyWords.Container;
using SteelLanguage.Token.Terms;

namespace ActionTest
{
    public class ProgramLibrary : ILibrary
    {
        public string Name => "program";

        public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
        {
            new Function("print", inputTypes:"string", action: terms =>
            {
                Console.WriteLine(terms[0].CastToStr());
            }),
            new Function("read", "string", terms =>
            {
                string read = Console.ReadLine();
                return new ReturnValue(read, "string");
            })
        };

        public IEnumerable<GlobalTerm> GlobalTerms => new[]
        {
            new GlobalTerm("program", "program_t")
        };
        
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }

        public ProgramLibrary(SystemLibrary baseLibrary)
        {
            TypeLibrary = new TypeLibrary();
            TermType type = baseLibrary.TypeLibrary.GetTermType("term");
            TermType enumType = baseLibrary.TypeLibrary.GetTermType("enum");
            
            TypeLibrary.AddTermType(new TermType(new Program.ProgramTerm(), type));
            TypeLibrary.AddTermType(new TermType(new Program.YuriEnum(), enumType));
        }
    }
    
    internal static class Program
    {
        private static readonly Dictionary<string, SteelScript> _compiled = new Dictionary<string, SteelScript>();
        private static readonly SteelCompiler _compiler = new SteelCompiler(new ProgramLibrary(SteelCompiler.Library));
        
        public class ProgramTerm : BaseTerm
        {
            public override string ValueType => "program_t";
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

            public override IEnumerable<IFunction> GetFunctions()
            {
                foreach (IFunction function in base.GetFunctions())
                {
                    yield return function;
                }

                yield return new Function("enumerate_test", "enumerable", () =>
                {
                    TermArray array = new TermArray("int", 4);
                    for (int i = 0; i < 4; i++)
                    {
                        TermI iTerm = new TermI()
                        {
                            TypeLibrary = TypeLibrary
                        };
                        iTerm.SetValue(i + 1);
                        array.SetValue(iTerm, i);
                    }

                    return new ReturnValue(array, "array<int>");
                });
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
        
        public class YuriEnum : EnumTerm
        {
            public override string ValueType => "yuri_kind";

            protected override string[] Values => new[]
            {
                "squid",
                "octo",
                "both"
            };
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

        private static SteelScript CompileScript(string fullPath)
        {
            FileInfo fileInfo = new FileInfo(fullPath);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("Specified file does not exist!");
                return null;
            }
            
            StreamReader reader = new StreamReader(fullPath);
            SteelScript script = _compiler.Compile(reader);

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