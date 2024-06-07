using System;
using System.Collections.Generic;
using System.IO;
using ActionScript.Functions;
using ActionScript.Library;
using ActionScript.Terms;

namespace ActionScript;

public class ActionCompiler
{
    private static readonly BaseActionLibrary BaseLibrary = new();

    private string _scriptPath;
    private ILibrary[] _libraries;
    private ActionScript _script;

    public ActionScript CompileScript()
    {
        _reader = new StringReader(_scriptPath);
        _script = new ActionScript();
        ImportLibrary(BaseLibrary);
        foreach (ILibrary library in _libraries)
        {
            ImportLibrary(library);
        }

        using (_reader)
        {
            string line = ReadCleanLine();
            while (line != null)
            {
                if (string.IsNullOrEmpty(line))
                {
                    line = ReadCleanLine();
                    continue;
                }
                
                ParseToken(line);
                line = ReadCleanLine();
            }
        }

        return _script;
    }
    
    #region Tokens

    public void ParseToken(string token)
    {
        if (token.StartsWith("func"))
        {
        }
        else if (token.StartsWith("term"))
        {
            Term term = ParseTermConstant(token);

            if (_script.Terms.ContainsKey(term.Name))
            {
                if (token.EndsWith(")"))
                {
                    FunctionCall functionCall = ParseFunctionCall(token.Split(new[] { '=' }, 2)[1].Trim());
                    _script.TokenCalls.Add(new AssignmentCall(term, new Input(functionCall), _script));
                }
                else
                {
                    _script.TokenCalls.Add(new AssignmentCall(_script.Terms[term.Name], new Input(term), _script));
                }
            }
            else
            {
                _script.Terms.Add(term.Name, term);
                if (token.EndsWith(")"))
                {
                    FunctionCall functionCall = ParseFunctionCall(token.Split(new[] { '=' }, 2)[1].Trim());
                    _script.TokenCalls.Add(new AssignmentCall(term, new Input(functionCall), _script));
                }
            }

            term.Line = _currentLine;
        }
        else // Likely a call of some form
        {
            FunctionCall call = ParseFunctionCall(token);
            call.Line = _currentLine;
            _script.TokenCalls.Add(call);
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
            else if (!_script.Terms.ContainsKey(input))
            {
                inputTokens.Add(new Input(input));
            }
            else
            {
                inputTokens.Add(new Input(_script.Terms[input]));
            }
        }

        return new FunctionCall(_script, _script.Functions[name], inputTokens);
    }

    #endregion

    #region Reading
    
    private int _currentLine;
    private TextReader _reader;

    private string ReadCleanLine()
    {
        _currentLine++;
        return CleanLine(_reader.ReadLine());
    }
    
    private string CleanLine(string line)
    {
        if (line == null) return null;
        
        line = line.Trim();

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

    #endregion

    #region Libraries

    public void ImportLibrary(ILibrary library)
    {
        if (library.Functions != null)
        {
            foreach (Function function in library.Functions)
            {
                if (!_script.Functions.ContainsKey(function.Name)) 
                    _script.Functions.Add(function.Name, function);
            }
        }

        if (library.GlobalTerms != null)
        {
            foreach (Term term in library.GlobalTerms)
            {
                _script.Terms.Add(term.Name, term);
            }
        }
    }

    #endregion

    public ActionCompiler(string path, params ILibrary[] libraries)
    {
        _scriptPath = path;
        _libraries = libraries;
    }
}