namespace Wpf.Ui.Violeta.Controls.Compat
{
    public interface IKeyIndexMapping
    {
        string KeyFromIndex(int index);
        int IndexFromKey(string key);
    }
}

