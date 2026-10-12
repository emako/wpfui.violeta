using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Wpf.Ui.Markdown.Plugins;

public class FilterChain : IConversion
{
    private readonly IFilter _filter;
    private readonly IConversion _next;

    public FilterChain(IFilter filter, IConversion next)
    {
        _filter = filter;
        _next = next;
    }

    public FlowDocument DoConversion(string markdownText)
    => _filter.DoFilter(markdownText, _next);

    public static FilterChain Create(IEnumerable<IFilter> filters, IConversion last)
    {
        var arr = filters.ToArray();
        if (arr.Length == 0) throw new ArgumentException();
        if (arr.Length == 1) return new FilterChain(arr[0], last);

        var chain = new FilterChain(arr[arr.Length - 1], last);
        for (var i = arr.Length - 2; i >= 0; i--)
        {
            chain = new FilterChain(arr[i], chain);
        }
        return chain;
    }
}

public static class FilterChainExtensions
{
    public static FilterChain ToFilterChain(this IEnumerable<IFilter> filters, IConversion last)
        => FilterChain.Create(filters, last);
}