using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.SpriteFont", "Microsoft.Xna.Framework.Content.SpriteFontReader")]
    public class SpriteFontReader : AsyncReader<SpriteFont>
    {
        public SpriteFontReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override async ValueTask<SpriteFont> ReadAsync(CancellationToken cancellationToken) => new(
            await Rx.ReadObjectNonNullAsync<Texture2D>(nameof(SpriteFont.Texture), cancellationToken),
            new(
                await Rx.ReadObjectNonNullAsync<ImmutableList<Rectangle>>(nameof(SpriteFontInfo.Glyphs), cancellationToken),
                await Rx.ReadObjectNonNullAsync<ImmutableList<Rectangle>>(nameof(SpriteFontInfo.Cropping), cancellationToken),
                await Rx.ReadObjectNonNullAsync<ImmutableList<char>>(nameof(SpriteFontInfo.CharacterMap), cancellationToken),
                Rx.ReadInt32(),
                Rx.ReadSingle(),
                await Rx.ReadObjectNonNullAsync<ImmutableList<Vector3>>(nameof(SpriteFontInfo.Kerning), cancellationToken),
                await Rx.ReadDirectAsync<char?>(cancellationToken)
            )
        );
    }
}
