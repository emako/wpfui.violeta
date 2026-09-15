namespace Wpf.Ui.Violeta.Controls.Svg.Shapes;

public interface ITextNode
{
    public CharacterLayout GetFirstCharacter();

    public CharacterLayout GetLastCharacter();

    public string GetText();

    public int GetLength();

    public CharacterLayout[] GetCharacters();
}
