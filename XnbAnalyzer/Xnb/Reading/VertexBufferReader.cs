using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.VertexBuffer", "Microsoft.Xna.Framework.Content.VertexBufferReader")]
public class VertexBufferReader : AsyncReader<VertexBuffer>
{
    public VertexBufferReader(XnbStreamReader rx) : base(rx) { }

    public override async ValueTask<VertexBuffer> ReadAsync(CancellationToken cancellationToken)
    {
        var declaration = await Rx.ReadDirectAsync<VertexDeclaration>(cancellationToken);
        var count = Rx.ReadUInt32();
        var data = new byte[checked((int)(count * declaration.Stride))];
        await Rx.ReadBytesAsync(data, cancellationToken);

        return new VertexBuffer(declaration, count, data.ToImmutableArray());
    }
}
