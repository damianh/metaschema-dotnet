// Licensed under the MIT License.

using Metaschema.Nodes;

namespace Metaschema.Serialization;

/// <summary>
/// Loads Metaschema-based content with automatic format detection.
/// </summary>
public sealed class BoundLoader : IBoundLoader
{
    private readonly IBindingContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundLoader"/> class.
    /// </summary>
    /// <param name="context">The binding context.</param>
    public BoundLoader(IBindingContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public IEnumerable<Format> EnabledFormats => [Format.Xml, Format.Json, Format.Yaml];

    /// <inheritdoc />
    public Format DetectFormat(Stream input)
    {
        if (!input.CanSeek)
        {
            throw new ArgumentException("Stream must be seekable for format detection.", nameof(input));
        }

        var startPosition = input.Position;

        try
        {
            // Read the first non-whitespace character
            int b;
            while ((b = input.ReadByte()) != -1)
            {
                var c = (char)b;
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                // Check for XML declaration or element start
                if (c == '<')
                {
                    return Format.Xml;
                }

                // Check for JSON object or array start
                if (c == '{' || c == '[')
                {
                    return Format.Json;
                }

                // Check for YAML indicators
                // YAML documents often start with '---' or a key (letter, number, or quote)
                if (c == '-' || c == '%' || char.IsLetter(c) || c == '"' || c == '\'')
                {
                    // Need to check if it's YAML or JSON with a string key
                    // Read more to determine
                    return DetectYamlOrJson(input, c, startPosition);
                }

                // Unknown format
                break;
            }

            throw new SerializationException("Unable to detect content format.");
        }
        finally
        {
            // Reset stream position
            input.Position = startPosition;
        }
    }

    private static Format DetectYamlOrJson(Stream input, char firstChar, long startPosition)
    {
        // If first char is a quote and we have a colon soon after, could be JSON
        // YAML typically uses unquoted keys followed by colon
        // For simplicity, if first meaningful char is a letter or quote, check for JSON object pattern

        if (firstChar == '-')
        {
            // Could be YAML document start (---) or YAML list item
            var next1 = input.ReadByte();
            var next2 = input.ReadByte();

            if (next1 == '-' && next2 == '-')
            {
                return Format.Yaml;
            }

            // YAML list item
            return Format.Yaml;
        }

        if (firstChar == '%')
        {
            // YAML directive
            return Format.Yaml;
        }

        // Read ahead to look for patterns
        input.Position = startPosition;
        using var reader = new StreamReader(input, leaveOpen: true);
        var buffer = new char[1024];
        var read = reader.Read(buffer, 0, buffer.Length);
        var content = new string(buffer, 0, read).TrimStart();

        // Check for common JSON patterns
        if (content.StartsWith('{') || content.StartsWith('['))
        {
            return Format.Json;
        }

        // Check for YAML document marker
        if (content.StartsWith("---", StringComparison.Ordinal))
        {
            return Format.Yaml;
        }

        // Check for key: value pattern (YAML style)
        if (content.Contains(':'))
        {
            var colonIndex = content.IndexOf(':');
            var beforeColon = content[..colonIndex].Trim();

            // If the key is unquoted, it's YAML
            if (!beforeColon.StartsWith('"') && !beforeColon.StartsWith('\''))
            {
                return Format.Yaml;
            }

            // If quoted and followed by colon without braces, still likely YAML
            // But if we see { before the key, it's JSON
            if (content.TrimStart().StartsWith('{'))
            {
                return Format.Json;
            }

            return Format.Yaml;
        }

        // Default to YAML for unstructured content
        return Format.Yaml;
    }

    /// <inheritdoc />
    public DocumentNode Load(Stream input)
    {
        if (!input.CanSeek)
        {
            // For non-seekable streams, read into memory first
            using var memoryStream = new MemoryStream();
            input.CopyTo(memoryStream);
            memoryStream.Position = 0;
            return Load(memoryStream);
        }

        var format = DetectFormat(input);
        return Load(input, format);
    }

    /// <inheritdoc />
    public DocumentNode Load(string path)
    {
        var format = DetectFormatFromExtension(path);
        using var stream = File.OpenRead(path);

        if (format.HasValue)
        {
            return Load(stream, format.Value);
        }

        return Load(stream);
    }

    /// <inheritdoc />
    public DocumentNode Load(Stream input, Format format)
    {
        var deserializer = _context.GetDeserializer(format);
        return deserializer.Deserialize(input);
    }

    private static Format? DetectFormatFromExtension(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".xml" => Format.Xml,
            ".json" => Format.Json,
            ".yaml" or ".yml" => Format.Yaml,
            _ => null
        };
    }
}
