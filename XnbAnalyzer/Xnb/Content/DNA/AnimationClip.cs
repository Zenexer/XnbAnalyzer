using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA
{
    public record AnimationClip(
        string Name,
        TimeSpan Duration,
        int AnimationFrameRate,
        ImmutableArray<ImmutableArray<Vector3>> Positions,
        ImmutableArray<ImmutableArray<Quaternion>> Rotations,
        ImmutableArray<ImmutableArray<Vector3>> Scales
    );
}
