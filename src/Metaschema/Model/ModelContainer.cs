// Licensed under the MIT License.

namespace Metaschema.Model;

/// <summary>
/// Base class for elements that can appear in a model.
/// </summary>
public abstract class ModelElement
{
}

/// <summary>
/// A container for model elements within an assembly definition.
/// </summary>
public sealed class ModelContainer
{
    /// <summary>
    /// Gets the model elements (field instances, assembly instances, choices, etc.).
    /// </summary>
    public IReadOnlyList<ModelElement> Elements { get; init; } = [];
}

/// <summary>
/// A choice group allows mutually exclusive selection of model elements.
/// </summary>
public sealed class ChoiceGroup : ModelElement
{
    /// <summary>
    /// Gets the choices available.
    /// </summary>
    public IReadOnlyList<ModelElement> Choices { get; init; } = [];
}
