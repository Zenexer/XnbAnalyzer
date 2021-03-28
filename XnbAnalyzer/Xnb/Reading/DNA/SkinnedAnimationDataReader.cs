using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA
{
    // sic
    [Reader("DNA.Drawing.Animation.SkinedAnimationData", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Animation.SkinedAnimationData]]")]
    public class SkinnedAnimationDataReader : Reader<SkinnedAnimationData>
    {
        public SkinnedAnimationDataReader(XnbStreamReader rx) : base(rx) { }

        public override SkinnedAnimationData Read()
        {
            var animationClips = Rx.ReadObject<ImmutableDictionary<string, AnimationClip>>() ?? throw new XnbFormatException($"Null reflective field: {nameof(AnimationData.AnimationClips)}");
            var inverseBindPose = Rx.ReadObject<ImmutableArray<Matrix4x4>>();
            var skeleton = Rx.ReadObject<Skeleton>() ?? throw new XnbFormatException($"Null reflective field: {nameof(SkinnedAnimationData.Skeleton)}");

            return new SkinnedAnimationData(animationClips, inverseBindPose, skeleton);
        }
    }
}
