using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.SpriteManager", "DNA.Drawing.SpriteManager+SpriteManagerReader")]
public class SpriteManagerReader : Reader<SpriteManager>
{
    public SpriteManagerReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override SpriteManager Read()
    {
        var texture = Rx.ReadObjectNonNull<Texture2D>(nameof(Sprite.Texture));
        var count = Rx.ReadInt32();
        var sprites = new Dictionary<string, Sprite>();

        for (var i = 0; i < count; i++)
        {
            var key = Rx.ReadString();
            var sourceRectangle = Rx.ReadDirect<Rectangle>();
            sprites[key] = new Sprite(texture, sourceRectangle);
        }

        return new SpriteManager(sprites.ToImmutableDictionary());
    }
}

