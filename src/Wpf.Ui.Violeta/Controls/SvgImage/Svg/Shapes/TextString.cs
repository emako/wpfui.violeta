namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

/// <summary>
/// The leaf of the Text tree
/// </summary>
[DebuggerDisplay("{Text}")]
public class TextString : ITextChild
{
    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.")]
    private static readonly Regex _trimmedWhitespace = new(@"\s+", RegexOptions.Compiled | RegexOptions.Singleline);

    /// <summary>
    /// Represents the layout info for the string of text owned by this TextString.
    /// </summary>
    public CharacterLayout[] Characters { get; set; }

    public Shape Parent { get; set; }

    /// <summary>
    /// The index of this TextString in the root TextShape. This is set by the <see cref="TextRender"/>.
    /// </summary>
    public int Index { get; internal set; }

    public TextString(Shape parent, string text)
    {
        Parent = parent;
        string trimmed = _trimmedWhitespace.Replace(text, " ");
        Characters = new CharacterLayout[trimmed.Length];
        for (int i = 0; i < trimmed.Length; i++)
        {
            var c = trimmed[i];
            Characters[i] = new CharacterLayout(c);
        }
    }

    public CharacterLayout GetFirstCharacter()
    {
        return Characters.FirstOrDefault()!;
    }

    public CharacterLayout GetLastCharacter()
    {
        return Characters.LastOrDefault()!;
    }

    public CharacterLayout FirstCharacter => GetFirstCharacter();
    public CharacterLayout LastCharacter => GetLastCharacter();
    public string Text => GetText();
    public int Length => GetLength();

    public TextStyle TextStyle { get; internal set; }

    public string GetText()
    {
        return new string([.. Characters.Select(c => c.Character)]);
    }

    public int GetLength()
    {
        return Characters.Length;
    }

    public CharacterLayout[] GetCharacters()
    {
        return Characters;
    }
}
