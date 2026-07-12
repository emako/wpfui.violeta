using System;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Wpf.Ui.DependencyPropertyGenerator;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class DependencyPropertyAttribute : Attribute
{
    public object DefaultValue { get; set; }

    public string DefaultValuePath { get; set; }

    public string PropertyChanged { get; set; }
}

#pragma warning restore IDE0130 // Namespace does not match folder structure
#pragma warning restore IDE0079 // Remove unnecessary suppression
