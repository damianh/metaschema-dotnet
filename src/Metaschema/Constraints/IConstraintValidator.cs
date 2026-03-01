// Copyright (c) Damian Hickey. All rights reserved.
// See LICENSE in the project root for license information.

using Metaschema.Metapath.Item;

namespace Metaschema.Constraints;

/// <summary>
/// Interface for validating constraints against content nodes.
/// </summary>
public interface IConstraintValidator
{
    /// <summary>
    /// Validates a single constraint against a node.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <param name="constraint">The constraint to evaluate.</param>
    /// <returns>Validation findings for this constraint.</returns>
    IEnumerable<ValidationFinding> Validate(INodeItem node, IConstraint constraint);

    /// <summary>
    /// Validates all constraints in a constraint set against a document root node.
    /// </summary>
    /// <param name="root">The document root node.</param>
    /// <param name="constraints">The constraints to validate.</param>
    /// <returns>All validation findings.</returns>
    ValidationResults ValidateAll(INodeItem root, IEnumerable<IConstraint> constraints);
}
