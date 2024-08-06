using System.Collections.Generic;
using AeroDynamicKerbalInterfaces.Controls.Fields;
using SteelLanguage;
using SteelLanguage.Reflection.Library;
using SteelLanguage.Reflection.Type;
using SteelLanguage.Token.Functions;
using SteelLanguage.Token.KeyWords.Container;
using UnityEngine;

namespace ProgrammableMod.Controls.CodeEditor;


// TODO: fix this
// The problem with this is unity's rich text system is very bad at handling editing text
// All it does is hide the html color stuff in the text from the ui, but it still exists to the editor
// so for example `<color=red>A</color>` will appear as just 'A' in the ui, but then when a user say backspaces it will delete the last character
// resulting in `<color=red>A</color`
// I dont know how to fix this lol
// the best thing I can think of is 
public class CodeHighlightAreaControl : TextAreaControl
{ 
    private static readonly string[] Colors = new[]
    {
        "#ffff80ff", // bracket/func color
        "#82AD5Aff", // keyword color
        "#4593D1ff" // term color
    };

    #region Stylers

    private readonly record struct ColorStyler(string A, string Color)
    {
        public string A { get; } = A;
        public string Color { get; } = Color;

        public string StyleText(string text) => text.Replace(A, $"<color={Color}>{A}</color>");
        public string CleanText(string text) => text.Replace($"<color={Color}>{A}</color>", A);
    }
    
    private static bool _generated = false;
    private static readonly List<ColorStyler> Replacers = new List<ColorStyler>(new []
    {
        new ColorStyler("{", Colors[0]),
        new ColorStyler("}", Colors[0]),
        new ColorStyler("(", Colors[0]),
        new ColorStyler(")", Colors[0]),
        new ColorStyler(" >= ", Colors[0]),
        new ColorStyler(" <= ", Colors[0]),
        new ColorStyler(" < ", Colors[0]),
        new ColorStyler(" > ", Colors[0]),
        new ColorStyler("!=", Colors[0]),
        new ColorStyler("==", Colors[0]),
        new ColorStyler("*", Colors[0]),
        new ColorStyler("+", Colors[0]),
        new ColorStyler("-", Colors[0]),
        new ColorStyler("/", Colors[0]),
        new ColorStyler("^", Colors[0]),
        new ColorStyler("new", Colors[1]),
        new ColorStyler("params", Colors[1]),
        new ColorStyler("null", Colors[2])
    });
    
    private void GenerateReplacers(SteelCompiler compiler)
    {
        if (_generated)
            return;
        
        _generated = true;
        
        foreach (ILibrary library in compiler.EnumerateLibraries())
        {
            if (library.Keywords != null)
            {
                foreach (IKeyword keyword in library.Keywords)
                {
                    Replacers.Add(new ColorStyler(keyword.Name, Colors[1]));
                }
            }

            if (library.GlobalFunctions != null)
            {
                foreach (IFunction function in library.GlobalFunctions)
                {
                    Replacers.Add(new ColorStyler($"{function.Name}(", Colors[1]));
                }
            }

            if (library.TypeLibrary != null)
            {
                foreach (TermType termType in library.TypeLibrary.EnumerateTypes())
                {
                    Replacers.Add(new ColorStyler(termType.Name, Colors[2]));
                    
                    foreach (IFunction function in termType.Functions)
                    {
                        Replacers.Add(new ColorStyler($".{function.Name}(", Colors[1]));
                    }
                }
            }
        }
    }

    #endregion

    private string _styledText;
    protected override void Draw()
    {
        string upd = GUILayout.TextArea(_styledText, GetStyle(), LayoutOptions);
        
        if (upd != _styledText)
        {
            Updated();
            Content.text = StripText(upd);
            _styledText = ThiccifyText(Content.text);
        }
    }

    private string ThiccifyText(string text)
    {
        foreach (ColorStyler styler in Replacers)
        {
            text = styler.StyleText(text);
        }

        return text;
    }

    private string StripText(string text)
    {
        foreach (ColorStyler styler in Replacers)
        {
            text = styler.CleanText(text);
        }

        return text;
    }

    public CodeHighlightAreaControl(int id, string content, SteelCompiler compiler) : base(id, content)
    {
        GenerateReplacers(compiler);
        LayoutOptions = new[]
        {
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        };

        _styledText = ThiccifyText(Content.text);

        WordWrap = false;
        RichText = true;
    }
}