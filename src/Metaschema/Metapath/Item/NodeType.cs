// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Metaschema.Metapath.Item;

/// <summary>
/// Represents the type of a node in the Metapath data model.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// A document node (the root of a document).
    /// </summary>
    Document,

    /// <summary>
    /// An assembly node (a complex, reusable structure).
    /// </summary>
    Assembly,

    /// <summary>
    /// A field node (a simple, named value with optional flags).
    /// </summary>
    Field,

    /// <summary>
    /// A flag node (an attribute-like simple value).
    /// </summary>
    Flag
}
