using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.BoundingBox", "Microsoft.Xna.Framework.Content.BoundingBoxReader")]
public class BoundingBoxReader : Reader<BoundingBox>
{
    public BoundingBoxReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override BoundingBox Read() => new(Rx.ReadVector3(), Rx.ReadVector3());
}
