using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Wpf.Ui.Violeta.Controls.Compat;

internal class ItemTemplateWrapper : IElementFactoryShim
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ItemTemplateWrapper(DataTemplate dataTemplate)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Template = dataTemplate;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public ItemTemplateWrapper(DataTemplateSelector dataTemplateSelector)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        TemplateSelector = dataTemplateSelector;
    }

    public DataTemplate Template { get; set; }

    public DataTemplateSelector TemplateSelector { get; set; }

    #region IElementFactory

    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        var selectedTemplate = Template ?? TemplateSelector.SelectTemplate(args.Data, null);
        // Check if selected template we got is valid
        // Still nullptr, fail with a reasonable message now.
        selectedTemplate ??= TemplateSelector.SelectTemplate(args.Data, null) ?? throw new InvalidOperationException("Null encountered as data template. That is not a valid value for a data template, and can not be used.");
        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        UIElement? element = null;

        if (recyclePool != null)
        {
            // try to get an element from the recycle pool.
            element = recyclePool.TryGetElement(string.Empty /* key */, (args.Parent as FrameworkElement)!);
        }

        if (element == null)
        {
            // no element was found in recycle pool, create a new element
            element = selectedTemplate.LoadContent() as FrameworkElement;

            // Template returned null, so insert empty element to render nothing
            element ??= new Rectangle
            {
                Width = 0,
                Height = 0,
            };

            // Associate template with element
            element.SetValue(RecyclePool.OriginTemplateProperty, selectedTemplate);
        }

        return element;
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        var element = args.Element;
        DataTemplate selectedTemplate = (Template ??
            element.GetValue(RecyclePool.OriginTemplateProperty) as DataTemplate)!;
        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        if (recyclePool == null)
        {
            // No Recycle pool in the template, create one.
            recyclePool = new RecyclePool();
            RecyclePool.SetPoolInstance(selectedTemplate, recyclePool);
        }

        recyclePool.PutElement(args.Element, string.Empty /* key */, args.Parent);
    }

    #endregion IElementFactory
}
