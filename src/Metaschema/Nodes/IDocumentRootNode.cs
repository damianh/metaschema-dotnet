// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Model;

namespace Metaschema.Nodes;

/// <summary>
/// Represents a document root node.
/// </summary>
public interface IDocumentRootNode : IDocumentNode
{
    /// <summary>
    /// Gets the assembly definition for this document root.
    /// </summary>
    AssemblyDefinition Definition { get; }
}
