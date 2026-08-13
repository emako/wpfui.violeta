using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Wpf.Ui.Appearance;

namespace Wpf.Ui.TextEditor.Controls;

public class CodeBox : ICSharpCode.AvalonEdit.TextEditor
{
    /// <summary>
    /// Low performance, use <see cref="ICSharpCode.AvalonEdit.TextEditor.Text"/> instead.
    /// </summary>
    public string Code
    {
        get => Text;
        set => Text = value;
    }

    public static readonly DependencyProperty CodeProperty =
        DependencyProperty.Register(nameof(Code), typeof(string), typeof(CodeBox), new PropertyMetadata(string.Empty, OnCodeChange));

    private static void OnCodeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ICSharpCode.AvalonEdit.TextEditor editor)
        {
            editor.Text = (string)e.NewValue;
        }
    }

    public bool LineWrap
    {
        get => WordWrap;
        set
        {
            if (value)
            {
                WordWrap = true;
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
            else
            {
                WordWrap = false;
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
    }

    public bool IsUseInnerSyntaxHightlighting
    {
        get => (bool)GetValue(IsUseInnerSyntaxHightlightingProperty);
        set => SetValue(IsUseInnerSyntaxHightlightingProperty, value);
    }

    public static readonly DependencyProperty IsUseInnerSyntaxHightlightingProperty =
        DependencyProperty.Register(nameof(IsUseInnerSyntaxHightlighting), typeof(bool), typeof(CodeBox), new PropertyMetadata(true, OnIsUseInnerSyntaxHightlightingChange));

    private static void OnIsUseInnerSyntaxHightlightingChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CodeBox editor)
        {
            if (editor.IsUseInnerSyntaxHightlighting)
            {
                editor.RegisterSyntaxHighlighting();
            }
        }
    }

    /// <summary>
    /// Support: JSON and so on.
    /// </summary>
    public string SyntaxHighlightingName
    {
        get => (string)GetValue(SyntaxHighlightingNameProperty);
        set => SetValue(SyntaxHighlightingNameProperty, value);
    }

    public static readonly DependencyProperty SyntaxHighlightingNameProperty =
        DependencyProperty.Register(nameof(SyntaxHighlightingName), typeof(string), typeof(CodeBox), new PropertyMetadata(string.Empty, OnSyntaxHighlightingNameChange));

    private static void OnSyntaxHighlightingNameChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ICSharpCode.AvalonEdit.TextEditor editor)
        {
            string? key = e.NewValue as string;

            editor.SyntaxHighlighting = string.IsNullOrWhiteSpace(key) ? null : HighlightingManager.Instance.GetDefinition(key);
        }
    }

    public CodeBox()
    {
        //
        // `TextArea.Caret.CaretBrush` of TextEditor is based on `TextBlock.ForegroundProperty`.
        //
        TextArea.SetResourceReference(TextArea.SelectionBrushProperty, "TextEditorSelectionBrush");
        TextArea.SelectionBorder = new(Brushes.Transparent, 0d);
        TextArea.SelectionCornerRadius = 2d;
        TextArea.SelectionForeground = null!;

        ApplicationThemeManager.Changed += ApplicationThemeManager_Changed;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        if (IsUseInnerSyntaxHightlighting)
        {
            RegisterSyntaxHighlighting();
            if (!string.IsNullOrWhiteSpace(SyntaxHighlightingName))
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(SyntaxHighlightingName);
            }
        }
    }

    private void ApplicationThemeManager_Changed(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        if (IsUseInnerSyntaxHightlighting)
        {
            RegisterSyntaxHighlighting();
            if (!string.IsNullOrWhiteSpace(SyntaxHighlightingName))
            {
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(SyntaxHighlightingName);
            }
        }
    }

    public void RegisterSyntaxHighlighting()
    {
        ApplicationTheme theme = ApplicationTheme.Light;

        if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark)
        {
            theme = ApplicationTheme.Dark;
        }
        else if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.HighContrast)
        {
            if (ApplicationThemeManager.GetSystemTheme() == SystemTheme.HCBlack)
            {
                theme = ApplicationTheme.Dark;
            }
        }

        string prefix = $"pack://application:,,,/Wpf.Ui.TextEditor;component/Resources/Syntax/{theme}";

        RegisterSyntaxHighlighting(ResourcesProvider.GetString($"{prefix}/JSON.xshd"), "JSON", [".json"]);
    }

    public void RegisterSyntaxHighlighting(string xshd, string name, string[] extensions)
    {
        using MemoryStream stream = new(Encoding.UTF8.GetBytes(xshd));
        using XmlReader reader = new XmlTextReader(stream);
        IHighlightingDefinition highlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

        HighlightingManager.Instance.RegisterHighlighting(name, extensions, highlighting);
    }

    public void ApplyRegisterSyntaxHighlighting(string name)
    {
        SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(name);
    }

    public IHighlightingDefinition GetRegisterSyntaxHighlighting(string name)
    {
        return HighlightingManager.Instance.GetDefinition(name);
    }
}

file static class ResourcesProvider
{
    static ResourcesProvider()
    {
        if (!UriParser.IsKnownScheme("pack"))
        {
            _ = PackUriHelper.UriSchemePack;
        }
    }

    public static string GetString(Uri uri, Encoding? encoding = null)
    {
        using Stream stream = Application.GetResourceStream(uri)?.Stream;
        using StreamReader streamReader = new(stream, encoding ?? Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    public static string GetString(string uriString, Encoding? encoding = null)
    {
        return GetString(new Uri(uriString), encoding);
    }
}
