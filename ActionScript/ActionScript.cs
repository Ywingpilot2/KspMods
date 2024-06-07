using System;
using System.Collections.Generic;
using System.Linq;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript
{
    public class ActionScript
    {
        private static readonly BaseActionLibrary _baseLibrary = new();
        
        public Dictionary<string, Function> Functions { get; }
        public Dictionary<string, Term> Terms { get; }
        
        public List<TokenCall> TokenCalls { get; }

        #region Tokens

        private int _currentLine;
        
        /// <summary>
        /// Parse a strings tokens into a set of executable actions
        /// </summary>
        /// <param name="tokens">the string to parse</param>
        public void ParseTokens(string tokens)
        {
            for (; _currentLine < tokens.Split('\n').Length; _currentLine++)
            {
                var s = tokens.Split('\n')[_currentLine];
                string line = CleanLine(s);
                if (line == "")
                    continue;
                
                ParseToken(line);
            }
        }

        public void ParseToken(string token)
        {
            if (token.StartsWith("func"))
            {
            }
            else if (token.StartsWith("term"))
            {
                Term term = ParseTermConstant(token);

                if (Terms.ContainsKey(term.Name))
                {
                    if (token.EndsWith(")"))
                    {
                        FunctionCall functionCall = ParseFunctionCall(token.Split(new[] { '=' }, 2)[1].Trim());
                        TokenCalls.Add(new AssignmentCall(term, new Input(functionCall), this));
                    }
                    else
                    {
                        TokenCalls.Add(new AssignmentCall(Terms[term.Name], new Input(term), this));
                    }
                }
                else
                {
                    Terms.Add(term.Name, term);
                    if (token.EndsWith(")"))
                    {
                        FunctionCall functionCall = ParseFunctionCall(token.Split(new[] { '=' }, 2)[1].Trim());
                        TokenCalls.Add(new AssignmentCall(term, new Input(functionCall), this));
                    }
                }

                term.Line = _currentLine;
            }
            else // Likely a call of some form
            {
                FunctionCall call = ParseFunctionCall(token);
                call.Line = _currentLine;
                TokenCalls.Add(call);
            }
        }

        #endregion

        #region Terms

        public Term ParseTermConstant(string token)
        {
            string[] split = token.Remove(0, 5).Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string name = split[0].Trim();
            
            string valueType = null;

            // Determine value type
            string declar = token.Split(new[] { ' ' }, 2)[0].Trim().ToLower();
            switch (declar)
            {
                case "termi":
                {
                    valueType = "int";
                } break;
                case "termf":
                {
                    valueType = "float";
                } break;
                case "termu":
                {
                    valueType = "uint";
                } break;
                case "termg":
                {
                    valueType = "guid";
                } break;
                case "terms":
                {
                    valueType = "string";
                } break;
            }

            if (split.Length == 1 || split[1].EndsWith(")"))
            {
                return new Term(name, null, valueType);
            }
            string value = split[1].Trim(' ', '"');

            if (valueType == null) // More type guessing
            {
                if (value.EndsWith("\""))
                {
                    valueType = "string";
                }
                else if (value.Contains("."))
                {
                    valueType = "float";
                }
                else if (value.Contains("-"))
                {
                    valueType = "guid"; // Thats a bit of a reach there buddy...
                }
                else if (value.ToLower() == "true" || value.ToLower() == "false")
                {
                    valueType = "bool";
                }
            }

            return new Term(name, value, valueType);
        }

        #endregion

        #region Functions

        public FunctionCall ParseFunctionCall(string token)
        {
            string[] split = token.Split(new []{'('}, 2);
            string name = split[0].Trim();
            string prms = split[1].Trim(' ', '\t');
            prms = prms.Remove(prms.Length - 1);

            List<string> inputs = new List<string>();

            string current = "";
            bool isStr = false;
            int methodLevel = 0;
            if (prms.Length > 1)
            {
                for (int i = 1; i < prms.Length; i++)
                {
                    char p = prms[i - 1];
                    char c = prms[i];

                    if (p == '"')
                    {
                        isStr = !isStr;
                    }

                    if (c == '(' && !isStr) // TODO: 
                    {
                        methodLevel++;
                    }

                    if (c == ')' && !isStr)
                    {
                        methodLevel--;
                    }

                    current += p;
                
                    if ((isStr || methodLevel != 0) && i + 1 < prms.Length)
                        continue;

                    if (c == ',' || i + 1 >= prms.Length)
                    {
                        current += c;

                        inputs.Add(current.Trim(' ', '\t', '"', ','));
                        current = "";
                    }
                }
            }
            else if (prms.Length != 0) // Lazy way to account for single character params lol
            {
                current = prms;
                inputs.Add(current.Trim(' ', '\t', '"'));
            }

            // Determine tokens
            List<Input> inputTokens = new List<Input>();
            foreach (string input in inputs)
            {
                if (input.EndsWith(")"))
                {
                    FunctionCall call = ParseFunctionCall(input);
                    inputTokens.Add(new Input(call));
                }
                else if (!Terms.ContainsKey(input))
                {
                    inputTokens.Add(new Input(input));
                }
                else
                {
                    inputTokens.Add(new Input(Terms[input]));
                }
            }

            return new FunctionCall(this, Functions[name], inputTokens);
        }

        #endregion

        #region Execution

        public void Execute()
        {
            foreach (TokenCall functionCall in TokenCalls)
            {
                functionCall.Call();
            }
        }

        #endregion
        
        private string CleanLine(string line)
        {
            line = line?.Trim();

            if (line == null) return "";
            
            int commentPosition = line.IndexOf("//", StringComparison.Ordinal); //Remove comments
            if (commentPosition != -1)
            {
                // Remove comment
                // We need to account for strings and such potentially containing // as well
                bool inStr = false;
                bool foundComment = false;

                for (var i = 1; i < line.Length; i++)
                {
                    if (foundComment)
                        break;
                        
                    char c = line[i];
                    char previous = line[i - 1];
                        
                    switch (c)
                    {
                        case '/':
                        {
                            if (!inStr && previous == '/')
                            {
                                commentPosition = i - 1;
                                foundComment = true;
                            }
                        } break;
                        case '"':
                        {
                            // Don't end the string if its escaped
                            if (inStr && previous != '\\')
                            {
                                inStr = false;
                            }
                            else
                            {
                                inStr = true;
                            }
                        } break;
                    }
                }

                if (foundComment)
                {
                    line = line.Remove(commentPosition).Trim();
                }
            }

            return line;
        }

        #region Constructors

        public ActionScript(params ILibrary[] libraries)
        {
            Functions = new Dictionary<string, Function>();
            Terms = new Dictionary<string, Term>();
            TokenCalls = new List<TokenCall>();

            ImportLibrary(_baseLibrary);
            foreach (ILibrary library in libraries)
            {
                ImportLibrary(library);
            }
        }

        public void ImportLibrary(ILibrary library)
        {
            if (library.Functions != null)
            {
                foreach (Function function in library.Functions)
                {
                    if (!Functions.ContainsKey(function.Name)) 
                        Functions.Add(function.Name, function);
                }
            }

            if (library.GlobalTerms != null)
            {
                foreach (Term term in library.GlobalTerms)
                {
                    Terms.Add(term.Name, term);
                }
            }
        }

        #endregion
    }
}