using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.BoundingSphere", "Microsoft.Xna.Framework.Content.BoundingSphereReader")]
    public class BoundingSphereReader : Reader<BoundingSphere>
    {
        public BoundingSphereReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override BoundingSphere Read() => new(
            Rx.ReadVector3(),
            Rx.ReadSingle()
        );
    }
}
