using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.VertexBuffer", "Microsoft.Xna.Framework.Content.VertexBufferReader")]
    public class VertexBufferReader : Reader<VertexBuffer>
    {
        public VertexBufferReader(XnbStreamReader rx) : base(rx) { }

        public override VertexBuffer Read()
        {
            var declaration = Rx.ReadDirect<VertexDeclaration>();
            var count = Rx.ReadUInt32();
            var data = Rx.ReadBytes(checked((int)(count * declaration.Stride)));

            return new VertexBuffer(declaration, count, data.ToImmutableArray());
        }
    }
}
