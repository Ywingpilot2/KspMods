using System;
using System.Collections.Generic;
using System.Linq;

namespace ActionScript.Extensions;

public static class StringExtension
{
    /// <summary>
    /// Splits a string ignoring characters in quotes 
    /// </summary>
    /// <param name="splitter">the character to use to split</param>
    /// <param name="count">The maximum number of splits</param>
    /// <param name="options">string split options</param>
    /// <returns>The split string</returns>
    public static string[] SmartSplit(this string self, char splitter, int count, StringSplitOptions options = StringSplitOptions.None)
    {
        List<string> splits = new List<string>(); // TODO: Use an array instead

        string current = "";
        bool isStr = false;
        bool isEsc = false;
        for (int i = 1; i < self.Length; i++)
        {
            char p = self[i - 1];
            char c = self[i];

            if ((p != splitter || splits.Count + 1 > count) || isStr)
            {
                current += p;
            }
            if (p == '"' && !isEsc)
                isStr = !isStr;

            if (p == '\\' || isEsc)
                isEsc = !isEsc;
            
            if (c == splitter && !isStr)
            {
                if (splits.Count + 1 > count)
                {
                    string last = splits.Last();
                    splits.Remove(last);
                    last += current;
                    splits.Add(last);
                }
                else
                {
                    splits.Add(current);
                }
                current = "";
            }

            if (i + 1 >= self.Length)
            {
                current += c;
            }
        }

        if (current != "")
        {
            if (splits.Count + 1 > count)
            {
                string last = splits.Last();
                splits.Remove(last);
                last += current;
                splits.Add(last);
            }
            else
            {
                splits.Add(current);
            }
        }

        if (options == StringSplitOptions.RemoveEmptyEntries && splits.Any(s => s == ""))
        {
            splits.RemoveAll(s => s == "");
        }

        return splits.ToArray();
    }
}