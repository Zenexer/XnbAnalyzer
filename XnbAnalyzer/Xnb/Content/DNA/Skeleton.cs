using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA
{
    public record Skeleton(
        ImmutableArray<Matrix4x4> Transforms,
        ImmutableArray<int> Hierarchy,
        ImmutableArray<string> Names
    );
}
