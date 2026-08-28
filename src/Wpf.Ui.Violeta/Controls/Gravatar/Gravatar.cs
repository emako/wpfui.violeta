using System.Windows;
using System.Windows.Controls;

namespace Wpf.Ui.Violeta.Controls;

/// <summary>
/// Displays a procedural identicon avatar generated from an <see cref="Id"/> string.
/// Image sources are not supported; use a custom <see cref="IGravatarGenerator"/> to change the pattern.
/// </summary>
public class Gravatar : ContentControl
{
    private static readonly IGravatarGenerator DefaultGenerator = new GithubGravatarGenerator();

    static Gravatar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Gravatar), new FrameworkPropertyMetadata(typeof(Gravatar)));
    }

    public Gravatar()
    {
        // Seed initial content when Id is set in XAML before Loaded.
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        if (Content == null)
        {
            UpdateContent();
        }
    }

    public static readonly DependencyProperty GeneratorProperty = DependencyProperty.Register(
        nameof(Generator),
        typeof(IGravatarGenerator),
        typeof(Gravatar),
        new PropertyMetadata(DefaultGenerator, OnGeneratorChanged));

    public IGravatarGenerator Generator
    {
        get => (IGravatarGenerator)GetValue(GeneratorProperty);
        set => SetValue(GeneratorProperty, value);
    }

    public static readonly DependencyProperty IdProperty = DependencyProperty.Register(
        nameof(Id),
        typeof(string),
        typeof(Gravatar),
        new PropertyMetadata(string.Empty, OnIdChanged));

    public string Id
    {
        get => (string)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius),
        typeof(CornerRadius),
        typeof(Gravatar),
        new PropertyMetadata(new CornerRadius(4)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static void OnIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Gravatar)d).UpdateContent();
    }

    private static void OnGeneratorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((Gravatar)d).UpdateContent();
    }

    private void UpdateContent()
    {
        var generator = Generator ?? DefaultGenerator;
        Content = generator.GetGravatar(Id);
    }
}
