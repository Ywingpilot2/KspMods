using System;
using System.Collections.Generic;
using System.Linq;

namespace SteelLanguage.Extensions;

public enum ScanDirection
{
    LeftToRight = 0,
    RightToLeft = 1
}

public static class StringExtension
{
    /// <summary>
    /// Splits a string ignoring characters in quotes 
    /// </summary>
    /// <param name="splitter">the character to use to split</param>
    /// <param name="count">The maximum number of splits</param>
    /// <param name="options">string split options</param>
    /// <returns>The split string</returns>
    public static string[] SanitizedSplit(this string self, char splitter, int count, StringSplitOptions options = StringSplitOptions.None, ScanDirection direction = ScanDirection.LeftToRight)
    {
        List<string> splits = new List<string>(); // TODO: Use an array instead

        string current = "";
        bool isStr = false;
        bool isEsc = false;
        if (direction == ScanDirection.LeftToRight)
        {
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
        }
        else
        {
            for (int i = self.Length - 2; i >= 0; i--)
            {
                char p = self[i + 1];
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
                        last = last.Insert(0, string.Join("", current.Reverse()));
                        splits.Add(last);
                    }
                    else
                    {
                        splits.Add(string.Join("", current.Reverse()));
                    }
                    current = "";
                }

                if (i - 1 < 0)
                {
                    current += c;
                }
            }
        }

        if (current != "")
        {
            if (splits.Count + 1 > count)
            {
                string last = splits.Last();
                splits.Remove(last);
                if (direction == ScanDirection.LeftToRight)
                {
                    last += current;
                }
                else
                {
                    last = last.Insert(0, string.Join("", current.Reverse()));
                }
                splits.Add(last);
            }
            else
            {
                if (direction == ScanDirection.LeftToRight)
                {
                    splits.Add(current);
                }
                else
                {
                    splits.Add(string.Join("", current.Reverse()));
                }
            }
        }

        if (direction == ScanDirection.RightToLeft)
        {
            splits.Reverse();
        }

        if (options == StringSplitOptions.RemoveEmptyEntries && splits.Any(s => s == ""))
        {
            splits.RemoveAll(s => s == "");
        }

        return splits.ToArray();
    }

    public static string[] SplitAt(this string self, int idx, int count = 2,
        StringSplitOptions options = StringSplitOptions.None, ScanDirection direction = ScanDirection.LeftToRight)
    {
        List<string> splits = new List<string>(); // TODO: Use an array instead

        string current = "";
        if (direction == ScanDirection.LeftToRight)
        {
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
        }
        else
        {
            for (int i = self.Length - 1; i >= 0; i--)
            {
                char c = self[i];
                current += c;

                if (i != idx) continue;
                if (splits.Count + 1 > count)
                {
                    string last = splits.Last();
                    splits.Remove(last);
                    last = last.Insert(0, string.Join("", current.Reverse()));
                    splits.Add(last);
                }
                else
                {
                    splits.Add(string.Join("", current.Reverse()));
                    current = "";
                }
            }
        }

        if (current != "")
        {
            if (splits.Count + 1 > count)
            {
                string last = splits.Last();
                splits.Remove(last);
                if (direction == ScanDirection.LeftToRight)
                {
                    last += current;
                }
                else
                {
                    last = last.Insert(0, string.Join("", current.Reverse()));
                }
                splits.Add(last);
            }
            else
            {
                if (direction == ScanDirection.LeftToRight)
                {
                    splits.Add(current);
                }
                else
                {
                    splits.Add(string.Join("", current.Reverse()));
                }
            }
        }
        
        if (direction == ScanDirection.RightToLeft)
        {
            splits.Reverse();
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
        if (self.Length > 1)
        {
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
        }
        else if (self.Length != 0)
        {
            if (self != "\"")
                return self;
        }

        return sanitized;
    }
    
    public static string SanitizeParenthesis(this string self)
    {
        string sanitized = ""; // omg green lady reference

        bool isStr = false;
        int level = 0;
        for (int i = 1; i < self.Length; i++)
        {
            char p = self[i - 1];
            char c = self[i];

            if (level == 0)
                sanitized += p;
            else
                sanitized += ' ';

            if (c == '(' && !isStr)
                level++;

            if (c == ')' && !isStr)
                level--;

            if (c == '"' && p != '\\')
                isStr = !isStr;
            
            if (i + 1 >= self.Length && c != ')')
            {
                sanitized += c;
            }
        }

        return sanitized;
    }

    public static bool SanitizedContains(this string self, string check)
    {
        return self.SanitizeQuotes().Contains(check);
    }

    public static int SanitizedIndexOf(this string self, string check, StringComparison comparison = StringComparison.Ordinal)
    {
        string santize = SanitizeQuotes(self);
        return santize.IndexOf(check, comparison);
    }

    public static string SanitizedReplace(this string self, string replace, string with)
    {
        string updated = self;
        if (self != replace)
        {
            // needs special handling to avoid forever replacing
            if (with.SanitizedContains(replace))
            {
                while (self.SanitizedContains(replace))
                {
                    string san = updated.SanitizeQuotes();
                    int idx = san.IndexOf(replace, san.IndexOf(with, StringComparison.Ordinal) + with.Length, StringComparison.Ordinal);
                    
                    updated = updated.Remove(idx, replace.Length);
                    updated = updated.Insert(idx, with);
                }
            }
            else
            {
                while (self.SanitizedContains(replace))
                {
                    int idx = SanitizedIndexOf(updated, replace);
                    updated = updated.Remove(idx, replace.Length);
                    updated = updated.Insert(idx, with);
                }
            }
        }
        else
        {
            return with;
        }

        return updated;
    }
}