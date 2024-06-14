using System;
using System.Collections.Generic;
using System.IO;
using ActionLanguage;
using ActionLanguage.Library;
using ActionLanguage.Token.Functions;
using ActionLanguage.Token.Interaction;
using ActionLanguage.Token.KeyWords;
using ActionLanguage.Token.Terms;

namespace ActionTest
{
    public class ProgramLibrary : ILibrary
    {
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
        public IEnumerable<BaseTerm> GlobalTerms { get; }
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }
    }
    
    internal static class Program
    {
        public static Dictionary<string, ActionScript> _compiled = new Dictionary<string, ActionScript>();

        public static void Main(string[] args)
        {
            bool interaction = false;
            
            while (true)
            {
                Console.WriteLine("Execute Action:");
                string input = Console.ReadLine();
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

                        ActionScript script = ActionExecution.CompileScriptFromFile(fileInfo.FullName, new ProgramLibrary());
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
                        ActionExecution.ExecuteFromFile(split[1].Trim('"'), new ProgramLibrary());
                    } break;
                    case "execLocal":
                    {
                        Console.WriteLine("Executing file...");
                        ExecuteLocal(split[1].Trim('"', ' ', '\t'));
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
            }
        }

        public static void ExecuteLocal(string relativePath)
        {
            ActionExecution.ExecuteFromFile($"{AppDomain.CurrentDomain.BaseDirectory}{relativePath}", new ProgramLibrary());
        }
    }
}