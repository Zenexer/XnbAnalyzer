using System;
using System.Collections.Immutable;

public record Node(
    string Name,
    ImmutableArray<Property> Properties,
    ImmutableArray<Node>? NestedList
)
{
}
