// Licensed under the MIT License.

namespace Metaschema.Databind.Binding.Attributes;

/// <summary>
/// Marks a class as a Metaschema assembly binding.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MetaschemaAssemblyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the definition name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the XML namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the root element name (if this is a document root).
    /// </summary>
    public string? RootName { get; set; }
}

/// <summary>
/// Marks a class as a Metaschema field binding.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class MetaschemaFieldAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the definition name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the XML namespace.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the data type name.
    /// </summary>
    public string? DataType { get; set; }
}

/// <summary>
/// Marks a property as bound to a flag instance.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class BoundFlagAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoundFlagAttribute"/> class.
    /// </summary>
    public BoundFlagAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundFlagAttribute"/> class.
    /// </summary>
    /// <param name="name">The flag name.</param>
    public BoundFlagAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the flag name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets whether the flag is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the data type name.
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Marks a property as bound to a field instance.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class BoundFieldAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoundFieldAttribute"/> class.
    /// </summary>
    public BoundFieldAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundFieldAttribute"/> class.
    /// </summary>
    /// <param name="name">The field name.</param>
    public BoundFieldAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the field name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the minimum occurrences.
    /// </summary>
    public int MinOccurs { get; set; }

    /// <summary>
    /// Gets or sets the maximum occurrences (-1 for unbounded).
    /// </summary>
    public int MaxOccurs { get; set; } = 1;

    /// <summary>
    /// Gets or sets the data type name.
    /// </summary>
    public string? DataType { get; set; }

    /// <summary>
    /// Gets or sets the group name for collections.
    /// </summary>
    public string? GroupAs { get; set; }

    /// <summary>
    /// Gets or sets the JSON behavior for collections.
    /// </summary>
    public JsonBehavior JsonBehavior { get; set; } = JsonBehavior.List;
}

/// <summary>
/// Marks a property as bound to an assembly instance.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class BoundAssemblyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BoundAssemblyAttribute"/> class.
    /// </summary>
    public BoundAssemblyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundAssemblyAttribute"/> class.
    /// </summary>
    /// <param name="name">The assembly name.</param>
    public BoundAssemblyAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the assembly name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the minimum occurrences.
    /// </summary>
    public int MinOccurs { get; set; }

    /// <summary>
    /// Gets or sets the maximum occurrences (-1 for unbounded).
    /// </summary>
    public int MaxOccurs { get; set; } = 1;

    /// <summary>
    /// Gets or sets the group name for collections.
    /// </summary>
    public string? GroupAs { get; set; }

    /// <summary>
    /// Gets or sets the JSON behavior for collections.
    /// </summary>
    public JsonBehavior JsonBehavior { get; set; } = JsonBehavior.List;
}

/// <summary>
/// Specifies the JSON field value key name for fields with a value and flags.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class JsonFieldValueKeyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFieldValueKeyAttribute"/> class.
    /// </summary>
    /// <param name="name">The JSON property name for the field value.</param>
    public JsonFieldValueKeyAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the JSON property name for the field value.
    /// </summary>
    public string Name { get; }
}

/// <summary>
/// Specifies the XML namespace for the bound element.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XmlNamespaceAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="XmlNamespaceAttribute"/> class.
    /// </summary>
    /// <param name="uri">The XML namespace URI.</param>
    public XmlNamespaceAttribute(string uri)
    {
        Uri = uri;
    }

    /// <summary>
    /// Gets the XML namespace URI.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// Gets or sets the namespace prefix.
    /// </summary>
    public string? Prefix { get; set; }
}

/// <summary>
/// Specifies collection grouping behavior for model instances.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class GroupAsAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupAsAttribute"/> class.
    /// </summary>
    /// <param name="name">The group name.</param>
    public GroupAsAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the group name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the JSON behavior.
    /// </summary>
    public JsonBehavior JsonBehavior { get; set; } = JsonBehavior.List;

    /// <summary>
    /// Gets or sets the XML wrapping behavior.
    /// </summary>
    public XmlWrappingBehavior XmlBehavior { get; set; } = XmlWrappingBehavior.Grouped;
}

/// <summary>
/// JSON collection behavior for grouped elements.
/// </summary>
public enum JsonBehavior
{
    /// <summary>
    /// Serialize as a JSON array.
    /// </summary>
    List,

    /// <summary>
    /// Serialize as a singleton value (unwrap single-item collections).
    /// </summary>
    SingletonOrList,

    /// <summary>
    /// Serialize as a keyed object using a flag as the key.
    /// </summary>
    ByKey
}

/// <summary>
/// XML wrapping behavior for grouped elements.
/// </summary>
public enum XmlWrappingBehavior
{
    /// <summary>
    /// Elements are wrapped in a group element.
    /// </summary>
    Grouped,

    /// <summary>
    /// Elements appear directly without a wrapper.
    /// </summary>
    Ungrouped
}
