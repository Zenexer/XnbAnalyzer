using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.Effect", "Microsoft.Xna.Framework.Content.EffectReader")]
    public class EffectReader : Reader<Effect>
    {
        public EffectReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override Effect Read()
        {
            var size = Rx.ReadUInt32();
            var bytecode = Rx.ReadBytes(checked((int)size));

            return new Effect(bytecode.ToImmutableArray());
        }
    }
}
