using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Violeta.Appearance;

namespace Wpf.Ui.Violeta.Converters;

/// <summary>
/// Pass-through <see cref="IValueConverter"/> that forces bindings to re-run <c>Convert</c>
/// when the application theme changes.
/// <para>
/// Insert it into a converter chain (for example LiteObservableConverters'
/// <c>ValueConverterGroup</c>) together with converters whose brush/color properties are set via
/// <c>DynamicResource</c>. Theme dictionary swaps update those properties, but the binding target
/// still holds the previous converted value until <see cref="BindingExpressionBase.UpdateTarget"/>
/// runs — this converter triggers that refresh.
/// </para>
/// </summary>
public sealed class ThemeRefreshConverter : IValueConverter
{
    private static bool _subscribed;

    public static ThemeRefreshConverter Instance { get; } = new();

    public ThemeRefreshConverter()
    {
        EnsureSubscribed();
    }

    private static void EnsureSubscribed()
    {
        if (_subscribed)
        {
            return;
        }

        _subscribed = true;
        ThemeManager.Changed += OnThemeChanged;
    }

    private static void OnThemeChanged(ApplicationTheme currentApplicationTheme, Color systemAccent)
    {
        _ = currentApplicationTheme;
        _ = systemAccent;

        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        _ = app.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(RefreshAll));
    }

    /// <summary>
    /// Manually refresh bindings whose converter chain contains <see cref="ThemeRefreshConverter"/>.
    /// </summary>
    public static void RefreshAll()
    {
        Application? app = Application.Current;
        if (app is null)
        {
            return;
        }

        foreach (Window window in app.Windows)
        {
            RefreshTree(window);
        }
    }

    private static void RefreshTree(DependencyObject? root)
    {
        if (root is null)
        {
            return;
        }

        RefreshObject(root);

        if (root is Popup { Child: DependencyObject popupChild })
        {
            RefreshTree(popupChild);
        }

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            RefreshTree(VisualTreeHelper.GetChild(root, i));
        }
    }

    private static void RefreshObject(DependencyObject d)
    {
        LocalValueEnumerator enumerator = d.GetLocalValueEnumerator();
        while (enumerator.MoveNext())
        {
            LocalValueEntry entry = enumerator.Current;
            if (!BindingOperations.IsDataBound(d, entry.Property))
            {
                continue;
            }

            BindingExpressionBase? expression = BindingOperations.GetBindingExpressionBase(d, entry.Property);
            if (expression is not null && ShouldRefresh(expression))
            {
                expression.UpdateTarget();
            }
        }
    }

    private static bool ShouldRefresh(BindingExpressionBase expression)
    {
        switch (expression)
        {
            case BindingExpression bindingExpression:
                return ContainsThemeRefresh(bindingExpression.ParentBinding?.Converter);
            case MultiBindingExpression multiBindingExpression:
                return ContainsThemeRefresh(multiBindingExpression.ParentMultiBinding?.Converter);
            case PriorityBindingExpression priorityBindingExpression:
                {
                    PriorityBinding? priorityBinding = priorityBindingExpression.ParentPriorityBinding;
                    if (priorityBinding is null)
                    {
                        return false;
                    }

                    foreach (BindingBase child in priorityBinding.Bindings)
                    {
                        if (child is Binding binding && ContainsThemeRefresh(binding.Converter))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            default:
                return false;
        }
    }

    private static bool ContainsThemeRefresh(object? converter)
    {
        if (converter is null)
        {
            return false;
        }

        if (converter is ThemeRefreshConverter)
        {
            return true;
        }

        // ValueConverterGroup / ValueConverterGroupExtension expose a Converters list.
        // Detect via reflection so this library does not take a hard dependency on LiteObservableConverters.
        PropertyInfo? convertersProperty = converter.GetType().GetProperty(
            "Converters",
            BindingFlags.Instance | BindingFlags.Public);

        if (convertersProperty?.GetValue(converter) is IEnumerable items)
        {
            foreach (object? item in items)
            {
                if (ContainsThemeRefresh(item))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        EnsureSubscribed();
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
