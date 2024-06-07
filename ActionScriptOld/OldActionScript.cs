using System;
using System.Collections.Generic;
using System.Globalization;

namespace ActionScript
{
    public class ActionScript
    {
        public Dictionary<string, Function> Functions { get; }
        public List<string> Terms { get; }

        /// <summary>
        /// A list of functions in order of execution
        /// </summary>
        public List<Call> ExecutingFunctions { get; }
        
        public Dictionary<string, Term> ExecutingTerms { get; }

        public bool ParseFromText(string text)
        {
            try
            {
                foreach (string s in text.Split('\n'))
                {
                    string line = ReadCleanLine(s);
                    if (string.IsNullOrEmpty(line))
                        continue;

                    ParseToken(line);
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        private void ParseToken(string line)
        {
            if (line.StartsWith("func"))
            {
                        
            }
            else if (line.StartsWith("term"))
            {
                        
            }
            else // function call
            {
                ParseFuncCall(line);
            }
        }

        private void ParseFuncCall(string line)
        {
            string funcName = "";
            List<FuncIn> funcIns = new List<FuncIn>();

            string[] split = line.Split('(');
            funcName = split[0].Trim(' ', '\t', '\n', '\r', '(');

            string ins = split[1].Trim(' ', '\t', '\n', '\r', ')');

            bool isStr = false;
            string name = "";
            List<string> terms = new List<string>();
            for (int i = 1; i < ins.Length; i++)
            {
                char prev = ins[i - 1];
                char cur = ins[i];

                if (prev == '"')
                {
                    isStr = !isStr;
                }

                name += prev;
                
                if (isStr)
                    continue;

                if (cur == ',')
                {
                    if (name.EndsWith(")"))
                    {
                        
                    }
                    else if (!terms.Contains(name))
                    {
                        
                    }
                    else // Constant
                    {
                        string id = Guid.NewGuid().ToString();
                        terms.Add(id);
                        
                    }
                }
            }
        }

        /// <summary>
        /// Reads the next line, also removes things like comments or whitespace
        /// </summary>
        /// <returns></returns>
        static string ReadCleanLine(string? line)
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

        public bool Execute()
        {
            try
            {
                foreach (Call call in ExecutingFunctions)
                {
                    call.ExecuteCall();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public ActionScript(Dictionary<string, Function> functions)
        {
            Functions = functions;
            ExecutingFunctions = new List<Call>();
            ExecutingTerms = new Dictionary<string, Term>();
        }

        public ActionScript()
        {
            Functions = new Dictionary<string, Function>();
            ExecutingFunctions = new List<Call>();
            ExecutingTerms = new Dictionary<string, Term>();
            
        }
    }
}