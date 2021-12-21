using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.Texture3D", "Microsoft.Xna.Framework.Content.Texture3DReader")]
public class Texture3DReader : AsyncReader<Texture3D>
{
    public Texture3DReader(XnbStreamReader rx)
        : base(rx) { }

    public override async ValueTask<Texture3D> ReadAsync(CancellationToken cancellationToken)
    {
        var surfaceFormat = Rx.ReadSurfaceFormat();
        var width = Rx.ReadUInt32();
        var height = Rx.ReadUInt32();
        var depth = Rx.ReadUInt32();
        var mipCount = Rx.ReadUInt32();

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

        return new Texture3D
        {
            SurfaceFormat = surfaceFormat,
            Width = width,
            Height = height,
            Depth = depth,
            MipImages = mipImages.ToImmutableArray(),
        };
    }
}
