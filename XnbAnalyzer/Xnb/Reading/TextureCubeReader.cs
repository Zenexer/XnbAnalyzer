using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.TextureCube", "Microsoft.Xna.Framework.Content.TextureCubeReader")]
public class TextureCubeReader : AsyncReader<TextureCube>
{
    public TextureCubeReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override async ValueTask<TextureCube> ReadAsync(CancellationToken cancellationToken)
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

            var bytes = new byte[dataSize];
            await Rx.ReadBytesAsync(bytes, cancellationToken);
            mipImages[i] = bytes.ToImmutableArray();
        }

        return new TextureCube
        {
            SurfaceFormat = surfaceFormat,
            Size = size,
            MipImages = mipImages.ToImmutableArray(),
        };
    }
}
