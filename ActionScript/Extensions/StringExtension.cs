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
    public static string[] SanitizedSplit(this string self, char splitter, int count, StringSplitOptions options = StringSplitOptions.None)
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

    public static string[] SplitAt(this string self, int idx, int count,
        StringSplitOptions options = StringSplitOptions.None)
    {
        List<string> splits = new List<string>(); // TODO: Use an array instead

        string current = "";
        for (int i = 0; i < self.Length; i++)
        {
            char c = self[i];
            current += c;

            if (i != idx) continue;
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
                current = "";
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

    /// <summary>
    /// This method will remove the contents of quotes and replace them with white space in order to make searching easier
    /// </summary>
    /// <returns>A sanitized string</returns>
    public static string SanitizeQuotes(this string self)
    {
        string sanitized = ""; // omg green lady reference

        bool isStr = false;
        for (int i = 1; i < self.Length; i++)
        {
            char p = self[i - 1];
            char c = self[i];

            if (!isStr)
                sanitized += p;
            else
                sanitized += ' ';

            if (c == '"' && p != '\\')
                isStr = !isStr;
            
            if (i + 1 >= self.Length && c != '"')
            {
                sanitized += c;
            }
        }

        return sanitized;
    }

    public static bool SanitizedContains(this string self, string check)
    {
        string clean = "";

        bool isStr = false;
        for (int i = 1; i < self.Length; i++)
        {
            char p = self[i - 1];
            char c = self[i];

            if (!isStr)
            {
                clean += p;
            }

            if (c == '"' && p != '\\')
                isStr = !isStr;
            
            if (i + 1 >= self.Length)
            {
                clean += c;
            }
        }

        return clean.Contains(check);
    }

    public static int SanitizedIndexOf(this string self, string check, StringComparison comparison = StringComparison.Ordinal)
    {
        string santize = SanitizeQuotes(self);
        return santize.IndexOf(check, comparison);
    }

    public static string SanitizedReplace(this string self, string replace, string with)
    {
        string updated = self;
        while (self.SanitizedContains(replace))
        {
            int idx = SanitizedIndexOf(updated, replace);
            updated = updated.Remove(idx, replace.Length);
            updated = updated.Insert(idx, with);
        }

        return updated;
    }
}