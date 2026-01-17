// Licensed under the MIT License.

using Metaschema.Core.Markup;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "markup-line" data type.
/// Single-line markup content supporting inline formatting.
/// </summary>
public sealed class MarkupLineAdapter : DataTypeAdapter<MarkupLine>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.MarkupLine;

    /// <inheritdoc />
    public override MarkupLine Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // MarkupLine accepts any non-null string
        // Full parsing/validation will be done in later phases with Markdig
        return new MarkupLine(value);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out MarkupLine result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        result = new MarkupLine(value);
        return true;
    }

    /// <inheritdoc />
    public override DataTypeValidationResult Validate(string value)
    {
        if (value is null)
        {
            return DataTypeValidationResult.Invalid("Value cannot be null");
        }

        // TODO: Add validation for inline markup in later phases
        return DataTypeValidationResult.Valid();
    }

    /// <inheritdoc />
    public override string Format(MarkupLine value) => value.Value;
}
