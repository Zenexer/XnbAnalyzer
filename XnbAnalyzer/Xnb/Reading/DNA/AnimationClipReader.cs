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
    [Reader("DNA.Drawing.Animation.AnimationClip", "DNA.Drawing.Animation.AnimationClip+Reader")]
    public class AnimationClipReader : Reader<AnimationClip>
    {
        public AnimationClipReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override AnimationClip Read()
        {
            var name = Rx.ReadString();
            var animationFrameRate = Rx.ReadInt32();
            var duration = TimeSpan.FromTicks(Rx.ReadInt64());

            var count = Rx.ReadInt32();
            var positions = new ImmutableArray<Vector3>[count];
            var rotations = new ImmutableArray<Quaternion>[count];
            var scales = new ImmutableArray<Vector3>[count];

            for (var x = 0; x < count; x++)
            {
                var countY = Rx.ReadInt32();
                var pos = new Vector3[countY];
                for (var y = 0; y < countY; y++)
                {
                    pos[y] = Rx.ReadVector3();
                }
                positions[x] = pos.ToImmutableArray();

                countY = Rx.ReadInt32();
                var rot = new Quaternion[countY];
                for (var y = 0; y < countY; y++)
                {
                    rot[y] = Rx.ReadQuaternion();
                }
                rotations[x] = rot.ToImmutableArray();

                countY = Rx.ReadInt32();
                var scale = new Vector3[countY];
                for (var y = 0; y < countY; y++)
                {
                    scale[y] = Rx.ReadVector3();
                }
                scales[x] = scale.ToImmutableArray();
            }

            return new AnimationClip(name, duration, animationFrameRate, positions.ToImmutableArray(), rotations.ToImmutableArray(), scales.ToImmutableArray());
        }
    }
}
