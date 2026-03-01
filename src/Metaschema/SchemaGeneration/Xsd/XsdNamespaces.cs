// Licensed under the MIT License.

using System.Xml.Linq;

namespace Metaschema.SchemaGeneration.Xsd;

/// <summary>
/// XML Schema namespace constants.
/// </summary>
internal static class XsdNamespaces
{
    /// <summary>
    /// The XML Schema namespace URI.
    /// </summary>
    public const string XsUri = "http://www.w3.org/2001/XMLSchema";

    /// <summary>
    /// The XML Schema namespace.
    /// </summary>
    public static readonly XNamespace Xs = XsUri;

    // Common XSD element names
    public static readonly XName Schema = Xs + "schema";
    public static readonly XName Element = Xs + "element";
    public static readonly XName Attribute = Xs + "attribute";
    public static readonly XName ComplexType = Xs + "complexType";
    public static readonly XName SimpleType = Xs + "simpleType";
    public static readonly XName Sequence = Xs + "sequence";
    public static readonly XName Choice = Xs + "choice";
    public static readonly XName All = Xs + "all";
    public static readonly XName ComplexContent = Xs + "complexContent";
    public static readonly XName SimpleContent = Xs + "simpleContent";
    public static readonly XName Extension = Xs + "extension";
    public static readonly XName Restriction = Xs + "restriction";
    public static readonly XName Annotation = Xs + "annotation";
    public static readonly XName Documentation = Xs + "documentation";
    public static readonly XName Pattern = Xs + "pattern";
    public static readonly XName MinLength = Xs + "minLength";
    public static readonly XName MaxLength = Xs + "maxLength";
    public static readonly XName Enumeration = Xs + "enumeration";
    public static readonly XName MinInclusive = Xs + "minInclusive";
    public static readonly XName MaxInclusive = Xs + "maxInclusive";
    public static readonly XName AnyAttribute = Xs + "anyAttribute";
    public static readonly XName Any = Xs + "any";
    public static readonly XName Import = Xs + "import";
    public static readonly XName Include = Xs + "include";
}
