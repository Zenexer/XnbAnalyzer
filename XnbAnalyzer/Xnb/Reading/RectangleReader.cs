using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Rectangle", "Microsoft.Xna.Framework.Content.RectangleReader")]
public class RectangleReader : SyncReader<Rectangle>
{
    public RectangleReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override Rectangle Read() => new(
        Rx.ReadInt32(),
        Rx.ReadInt32(),
        Rx.ReadInt32(),
        Rx.ReadInt32()
    );
}
