using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.TextureCube", "Microsoft.Xna.Framework.Content.TextureCubeReader")]
public class TextureCubeReader : Reader<TextureCube>
{
    public TextureCubeReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override TextureCube Read()
    {
        var surfaceFormat = Rx.ReadSurfaceFormat();
        var size = Rx.ReadUInt32();
        var mipCount = Rx.ReadUInt32() * 6;

        if (mipCount >= 32)
        {
            throw new XnbFormatException("Too many mip levels");
        }

        var mipImages = new ImmutableArray<byte>[(int)mipCount];

        for (var i = 0; i < mipCount; i++)
        {
            var dataSize = Rx.ReadUInt32();

            if (dataSize >= (1 << 30))  // 1 GiB
            {
                throw new XnbFormatException("Mipmap is excessively large");
            }

            var imageData = Rx.ReadBytes((int)dataSize).ToImmutableArray();

            mipImages[i] = imageData;
        }

        return new TextureCube
        {
            SurfaceFormat = surfaceFormat,
            Size = size,
            MipImages = mipImages.ToImmutableArray(),
        };
    }
}
