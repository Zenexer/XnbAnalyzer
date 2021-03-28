using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA
{
    public record AnimationData(ImmutableDictionary<string, AnimationClip> AnimationClips);
}
