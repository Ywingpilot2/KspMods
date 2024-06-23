using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ActionLanguage;
using ActionLanguage.Library;
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
        public IEnumerable<GlobalTerm> GlobalTerms { get; }
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }
    }
    
    internal static class Program
    {
        private static Dictionary<string, ActionScript> _compiled = new Dictionary<string, ActionScript>();
        private static ActionCompiler _compiler = new ActionCompiler(new ProgramLibrary());

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
                        FileInfo fileInfo = new FileInfo(global);
                        if (!fileInfo.Exists)
                        {
                            Console.WriteLine("Specified file does not exist!");
                            continue;
                        }

                        ActionScript script = _compiler.Compile(new StreamReader(fileInfo.FullName));
                        if (_compiled.ContainsKey(fileInfo.Name))
                        {
                            _compiled.Remove(fileInfo.Name);
                        }
                        _compiled.Add(fileInfo.Name, script);
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
                        ActionScript script = _compiler.Compile(new StreamReader(split[1].Trim('"', ' ')));
                        script.Execute();
                    } break;
                    case "execLocal":
                    {
                        Console.WriteLine("Executing file...");
                        ActionScript script = _compiler.Compile(new StreamReader(GetGlobalPath(split[1].Trim('"', ' '))));
                        script.Execute();
                    } break;
                    case "begin-interaction":
                    {
                        interaction = true;
                        Console.WriteLine("Beginning AS-Interactive...");
                    } break;
                    case "exit":
                    {
                        return;
                    } break;
                }
                
                stopwatch.Stop();
                Console.WriteLine($"Executed action in {stopwatch.Elapsed}");
                stopwatch.Reset();
            }
        }

        public static string GetGlobalPath(string relativePath) => $"{AppDomain.CurrentDomain.BaseDirectory}{relativePath}";
    }
}