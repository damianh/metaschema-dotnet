// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

namespace Metaschema.Model;

/// <summary>
/// Visibility scope for definitions.
/// </summary>
public enum Scope
{
    /// <summary>Definition is available for import by other modules.</summary>
    Global,

    /// <summary>Definition is only usable within the defining module.</summary>
    Local
}
