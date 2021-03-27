using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalzyer.Xnb.Content;

namespace XnbAnalzyer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.Texture3D", "Microsoft.Xna.Framework.Content.Texture3DReader")]
    public class Texture3DReader : Reader<Texture3D>
    {
        public Texture3DReader(XnbStreamReader rx)
            : base(rx) { }

        public override Texture3D Read()
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

                var imageData = Rx.ReadBytes((int)dataSize).ToImmutableArray();

                mipImages[i] = imageData;
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
}
