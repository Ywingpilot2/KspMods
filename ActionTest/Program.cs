﻿using System;
using System.Collections.Generic;
using ActionScript;
using ActionScript.Library;
using ActionScript.Token.Functions;
using ActionScript.Token.Interaction;
using ActionScript.Token.KeyWords;
using ActionScript.Token.Terms;

namespace ActionTest
{
    public class ProgramLibrary : ILibrary
    {
        public IEnumerable<IFunction> GlobalFunctions => new IFunction[]
        {
            new Function("print", "string", inputTypes:"string", action: terms =>
            {
                Console.WriteLine(terms[0].CastToStr());
                return new ReturnValue();
            })
        };
        public IEnumerable<BaseTerm> GlobalTerms { get; }
        public IEnumerable<IKeyword> Keywords { get; }
        public TypeLibrary TypeLibrary { get; }
    }
    
    internal class Program
    {
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