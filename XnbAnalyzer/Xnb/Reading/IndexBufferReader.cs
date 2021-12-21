using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.IndexBuffer", "Microsoft.Xna.Framework.Content.IndexBufferReader")]
    public class IndexBufferReader : SyncReader<IndexBuffer>
    {
        public IndexBufferReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override IndexBuffer Read()
        {
            var is16Bit = Rx.ReadBoolean();
            var size = Rx.ReadUInt32();
            var data = new byte[size];

            for (var i = 0; i < size; i++)
            {
                data[i] = Rx.ReadByte();
            }

            return new IndexBuffer(is16Bit, data.ToImmutableArray());
        }
    }
}
