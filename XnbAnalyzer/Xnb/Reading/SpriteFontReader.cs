using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.SpriteFont", "Microsoft.Xna.Framework.Content.SpriteFontReader")]
    public class SpriteFontReader : Reader<SpriteFont>
    {
        public SpriteFontReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override SpriteFont Read() => new(
            Rx.ReadObjectNonNull<Texture2D>(nameof(SpriteFont.Texture)),
            new(
                Rx.ReadObjectNonNull<ImmutableList<Rectangle>>(nameof(SpriteFontInfo.Glyphs)),
                Rx.ReadObjectNonNull<ImmutableList<Rectangle>>(nameof(SpriteFontInfo.Cropping)),
                Rx.ReadObjectNonNull<ImmutableList<char>>(nameof(SpriteFontInfo.CharacterMap)),
                Rx.ReadInt32(),
                Rx.ReadSingle(),
                Rx.ReadObjectNonNull<ImmutableList<Vector3>>(nameof(SpriteFontInfo.Kerning)),
                Rx.ReadDirect<char?>()
            )
        );
    }
}
