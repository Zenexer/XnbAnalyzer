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
    [Reader("DNA.Drawing.Skeleton", "DNA.Drawing.Skeleton+Reader")]
    public class SkeletonReader : Reader<Skeleton>
    {
        public SkeletonReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override Skeleton Read()
        {
            var count = Rx.ReadInt32();
            var names = new string[count];
            var transforms = new Matrix4x4[count];
            var hierarchy = new int[count];

            for (var i = 0; i < count; i++)
            {
                names[i] = Rx.ReadString();
                hierarchy[i] = Rx.ReadInt32();
                transforms[i] = Rx.ReadMatrix4x4();
            }

            return new Skeleton(transforms.ToImmutableArray(), hierarchy.ToImmutableArray(), names.ToImmutableArray());
        }
    }
}
