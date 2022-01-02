using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.Effect", "Microsoft.Xna.Framework.Content.EffectReader")]
public class EffectReader : AsyncReader<Effect>
{
    public EffectReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override async ValueTask<Effect> ReadAsync(CancellationToken cancellationToken)
    {
        var size = Rx.ReadUInt32();
        var bytecode = await Rx.ReadBytesAsync(size, cancellationToken);

        return new Effect(bytecode);
    }
}
