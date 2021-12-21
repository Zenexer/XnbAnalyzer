using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.BasicEffect", "Microsoft.Xna.Framework.Content.BasicEffectReader")]
public class BasicEffectReader : SyncReader<BasicEffect>
{
    public BasicEffectReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override BasicEffect Read() => new(
        Rx.ReadExternalReference<Texture>(),
        Rx.ReadVector3(),
        Rx.ReadVector3(),
        Rx.ReadVector3(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadBoolean()
    );
}
