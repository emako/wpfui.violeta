namespace iNKORE.UI.WPF.Modern.Controls
{
    public interface IKeyIndexMapping
    {
        string KeyFromIndex(int index);
        int IndexFromKey(string key);
    }
}
