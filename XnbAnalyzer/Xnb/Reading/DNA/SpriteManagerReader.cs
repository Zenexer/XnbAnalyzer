using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.SpriteManager", "DNA.Drawing.SpriteManager+SpriteManagerReader")]
public class SpriteManagerReader : AsyncReader<SpriteManager>
{
    public SpriteManagerReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override async ValueTask<SpriteManager> ReadAsync(CancellationToken cancellationToken)
    {
        var texture = await Rx.ReadObjectNonNullAsync<Texture2D>(nameof(Sprite.Texture), cancellationToken);
        var count = Rx.ReadInt32();
        var sprites = new Dictionary<string, Sprite>();

        for (var i = 0; i < count; i++)
        {
            var key = Rx.ReadString();
            var sourceRectangle = await Rx.ReadDirectAsync<Rectangle>(cancellationToken);
            sprites[key] = new Sprite(texture, sourceRectangle);
        }

        return new SpriteManager(sprites.ToImmutableDictionary());
    }
}

