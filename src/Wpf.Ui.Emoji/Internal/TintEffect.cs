using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Wpf.Ui.Emoji;

public class TintEffect : ShaderEffect
{
    static TintEffect()
    {
        m_shader = new PixelShader
        {
            UriSource = new Uri(@"/Wpf.Ui.Emoji;component/Resources/Shaders/TintEffect.ps", UriKind.Relative)
        };
    }

    public TintEffect()
    {
        PixelShader = m_shader;
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(TintProperty);
    }

    public static readonly DependencyProperty TintProperty =
        DependencyProperty.Register(nameof(Tint), typeof(Color), typeof(TintEffect),
            new UIPropertyMetadata(Colors.Red, PixelShaderConstantCallback(0)));

    public static readonly DependencyProperty InputProperty =
        RegisterPixelShaderSamplerProperty(nameof(Input), typeof(TintEffect), 0);

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public Color Tint
    {
        get => (Color)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    private static readonly PixelShader m_shader;
}
