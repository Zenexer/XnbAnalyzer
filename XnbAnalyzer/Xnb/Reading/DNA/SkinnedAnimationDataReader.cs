using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA
{
    // sic
    [Reader("DNA.Drawing.Animation.SkinedAnimationData", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Animation.SkinedAnimationData]]")]
    public class SkinnedAnimationDataReader : AsyncReader<SkinnedAnimationData>
    {
        public SkinnedAnimationDataReader(XnbStreamReader rx) : base(rx) { }

        public override async ValueTask<SkinnedAnimationData> ReadAsync(CancellationToken cancellationToken)
        {
            var animationClips = await Rx.ReadObjectNonNullAsync<ImmutableDictionary<string, AnimationClip>>(nameof(AnimationData.AnimationClips), cancellationToken);
            var inverseBindPose = await Rx.ReadObjectAsync<ImmutableArray<Matrix4x4>>(cancellationToken);
            var skeleton = await Rx.ReadObjectNonNullAsync<Skeleton>(nameof(SkinnedAnimationData.Skeleton), cancellationToken);

            return new SkinnedAnimationData(animationClips, inverseBindPose, skeleton);
        }
    }
}
