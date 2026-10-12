using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Wpf.Ui.Markdown.Appearance;

internal class ResourceDictionaryManager(string searchNamespace)
{
    public string SearchNamespace { get; } = searchNamespace;

    public bool UpdateDictionary(string resourceLookup, Uri? newResourceUri)
    {
        Collection<ResourceDictionary> applicationDictionaries = UiApplication
            .Current
            .Resources
            .MergedDictionaries;

        if (applicationDictionaries.Count == 0 || newResourceUri is null)
        {
            return false;
        }

        resourceLookup = resourceLookup.ToLower().Trim();

        for (var i = 0; i < applicationDictionaries.Count; i++)
        {
            string sourceUri;

            if (applicationDictionaries[i]?.Source != null)
            {
                sourceUri = applicationDictionaries[i].Source.ToString().ToLower().Trim();

                if (sourceUri.Contains(SearchNamespace) && sourceUri.Contains(resourceLookup))
                {
                    applicationDictionaries[i] = new() { Source = newResourceUri };

                    return true;
                }
            }

            for (var j = 0; j < applicationDictionaries[i].MergedDictionaries.Count; j++)
            {
                if (applicationDictionaries[i].MergedDictionaries[j]?.Source == null)
                {
                    continue;
                }

                sourceUri = applicationDictionaries[i]
                    .MergedDictionaries[j]
                    .Source.ToString()
                    .ToLower()
                    .Trim();

                if (!sourceUri.Contains(SearchNamespace) || !sourceUri.Contains(resourceLookup))
                {
                    continue;
                }

                applicationDictionaries[i].MergedDictionaries[j] = new() { Source = newResourceUri };

                return true;
            }
        }

        return false;
    }
}
