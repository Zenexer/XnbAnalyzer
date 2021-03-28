using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.Texture2D", "Microsoft.Xna.Framework.Content.Texture2DReader")]
    public class Texture2DReader : Reader<Texture2D>
    {
        public Texture2DReader(XnbStreamReader rx)
            : base(rx) { }

        public override Texture2D Read()
        {
            var surfaceFormat = Rx.ReadSurfaceFormat();
            var width = Rx.ReadUInt32();
            var height = Rx.ReadUInt32();

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

            return new Texture2D
            {
                SurfaceFormat = surfaceFormat,
                Width = width,
                Height = height,
                MipImages = mipImages.ToImmutableArray(),
            };
        }
    }
}
