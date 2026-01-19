// Licensed under the MIT License.
// Hand-crafted OSCAL Catalog types - these represent what the source generator would produce.

namespace Oscal.Typed.HandCrafted;

public class Catalog
{
    public Guid Uuid { get; set; }
    public Metadata? Metadata { get; set; }
    public List<Group> Groups { get; set; } = [];
    public BackMatter? BackMatter { get; set; }
}

public class Metadata
{
    public string? Title { get; set; }
    public DateTimeOffset? Published { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? Version { get; set; }
    public string? OscalVersion { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Role> Roles { get; set; } = [];
    public List<Party> Parties { get; set; } = [];
    public string? Remarks { get; set; }
}

public class Group
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Parameter> Params { get; set; } = [];
    public List<Part> Parts { get; set; } = [];
    public List<Control> Controls { get; set; } = [];
    public List<Group> NestedGroups { get; set; } = [];
}

public class Control
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Parameter> Params { get; set; } = [];
    public List<Part> Parts { get; set; } = [];
    public List<Control> Enhancements { get; set; } = [];
}

public class Parameter
{
    public string? Id { get; set; }
    public string? Class { get; set; }
    public string? Label { get; set; }
    public string? Usage { get; set; }
    public List<string> Values { get; set; } = [];
    public string? Select { get; set; }
    public string? Remarks { get; set; }
}

public class Part
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Ns { get; set; }
    public string? Class { get; set; }
    public string? Title { get; set; }
    public string? Prose { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public List<Part> SubParts { get; set; } = [];
}

public class OscalProperty
{
    public string? Name { get; set; }
    public Guid? Uuid { get; set; }
    public string? Ns { get; set; }
    public string? Value { get; set; }
    public string? Class { get; set; }
    public string? Remarks { get; set; }
}

public class Link
{
    public Uri? Href { get; set; }
    public string? Rel { get; set; }
    public string? MediaType { get; set; }
    public string? Text { get; set; }
}

public class Role
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? ShortName { get; set; }
    public string? Description { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public List<Link> Links { get; set; } = [];
    public string? Remarks { get; set; }
}

public class Party
{
    public Guid Uuid { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public List<string> EmailAddresses { get; set; } = [];
    public List<string> TelephoneNumbers { get; set; } = [];
    public string? Remarks { get; set; }
}

public class BackMatter
{
    public List<Resource> Resources { get; set; } = [];
}

public class Resource
{
    public Guid Uuid { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<OscalProperty> Props { get; set; } = [];
    public string? Remarks { get; set; }
}
