// Licensed under the MIT License.

using Metaschema.Core.Markup;

namespace Metaschema.Core.Datatypes.Adapters;

/// <summary>
/// Adapter for the Metaschema "markup-multiline" data type.
/// Multi-line markup content supporting block-level and inline formatting.
/// </summary>
public sealed class MarkupMultilineAdapter : DataTypeAdapter<MarkupMultiline>
{
    /// <inheritdoc />
    public override string TypeName => MetaschemaDataTypes.MarkupMultiline;

    /// <inheritdoc />
    public override MarkupMultiline Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // MarkupMultiline accepts any non-null string
        // Full parsing/validation will be done in later phases with Markdig
        return new MarkupMultiline(value);
    }

    /// <inheritdoc />
    public override bool TryParse(string value, out MarkupMultiline result)
    {
        if (value is null)
        {
            result = default;
            return false;
        }

        result = new MarkupMultiline(value);
        return true;
    }

    /// <inheritdoc />
    public override DataTypeValidationResult Validate(string value)
    {
        if (value is null)
        {
            return DataTypeValidationResult.Invalid("Value cannot be null");
        }

        // TODO: Add validation for block/inline markup in later phases
        return DataTypeValidationResult.Valid();
    }

    /// <inheritdoc />
    public override string Format(MarkupMultiline value) => value.Value;
}
