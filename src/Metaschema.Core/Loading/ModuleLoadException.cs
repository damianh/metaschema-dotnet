// Licensed under the MIT License.

namespace Metaschema.Core.Loading;

/// <summary>
/// Exception thrown when a module fails to load.
/// </summary>
public class ModuleLoadException : MetaschemaException
{
    /// <summary>
    /// Gets the location of the module that failed to load.
    /// </summary>
    public Uri? Location { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoadException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ModuleLoadException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoadException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="location">The location of the module.</param>
    public ModuleLoadException(string message, Uri location)
        : base(message)
    {
        Location = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleLoadException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="location">The location of the module.</param>
    /// <param name="innerException">The inner exception.</param>
    public ModuleLoadException(string message, Uri location, Exception innerException)
        : base(message, innerException)
    {
        Location = location;
    }
}
