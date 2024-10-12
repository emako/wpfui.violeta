using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Wpf.Ui.Violeta.Appearance;

internal class ResourceDictionaryManager
{
    public string SearchNamespace { get; }

    public ResourceDictionaryManager(string searchNamespace)
    {
        SearchNamespace = searchNamespace;
    }

    public bool HasDictionary(string resourceLookup)
    {
        return GetDictionary(resourceLookup) != null;
    }

    public ResourceDictionary? GetDictionary(string resourceLookup)
    {
        Collection<ResourceDictionary> applicationDictionaries = GetApplicationMergedDictionaries();

        if (applicationDictionaries.Count == 0)
        {
            return null;
        }

        resourceLookup = resourceLookup.ToLower().Trim();

        foreach (ResourceDictionary t in applicationDictionaries)
        {
            string resourceDictionaryUri;

            if (t?.Source != null)
            {
                resourceDictionaryUri = t.Source.ToString().ToLower().Trim();

                if (
                    resourceDictionaryUri.Contains(SearchNamespace)
                    && resourceDictionaryUri.Contains(resourceLookup)
                )
                {
                    return t;
                }
            }

            foreach (ResourceDictionary? t1 in t!.MergedDictionaries)
            {
                if (t1?.Source == null)
                {
                    continue;
                }

                resourceDictionaryUri = t1.Source.ToString().ToLower().Trim();

                if (
                    !resourceDictionaryUri.Contains(SearchNamespace)
                    || !resourceDictionaryUri.Contains(resourceLookup)
                )
                {
                    continue;
                }

                return t1;
            }
        }

        return null;
    }

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

    private Collection<ResourceDictionary> GetApplicationMergedDictionaries()
    {
        return UiApplication.Current.Resources.MergedDictionaries;
    }
}
