using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public class Texture2D : Texture
    {
        public SurfaceFormat SurfaceFormat { get; init; }
        public uint Width { get; init; }
        public uint Height { get; init; }
        public ImmutableArray<ImmutableArray<byte>> MipImages { get; init; }

        public override string ToString() =>
            $"{GetType().Name}: {Width}x{Height}, {SurfaceFormat}, {MipImages.Length} level(s)";

        public IEnumerable<Image> ToImages()
        {
            var w = checked((int)Width);
            var h = checked((int)Height);

            for (var i = 0; i < MipImages.Length; i++, w >>= 1, h >>= 1)
            {
                yield return ToImage(w == 0 ? 1 : w, h == 0 ? 1 : h, MipImages[i].AsSpan());
            }
        }

        private Image ToImage(int width, int height, ReadOnlySpan<byte> data) => SurfaceFormat switch
        {
            SurfaceFormat.Color => ToImage_Color(width, height, data),
            SurfaceFormat.Dxt1 => ToImage_Dxt1(width, height, data),
            SurfaceFormat.Dxt5 => ToImage_Dxt5(width, height, data),
            _ => throw new NotImplementedException($"Surface format not yet implemented: {SurfaceFormat}"),
        };

        public override async Task SaveToFolderAsync(string dir, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(dir);

            var layer = 0;

            foreach (var image in ToImages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var name = $"{layer} - {image.Width}x{image.Height}.png";
                var path = Path.Combine(dir, name);

                await image.SaveAsPngAsync(path, cancellationToken);

                layer++;
            }
        }

        private static Image<Rgba32> ToImage_Color(int width, int height, ReadOnlySpan<byte> data)
        {
            var image = new Image<Rgba32>(width, height);
            var i = 0;

            for (var y = 0; y < height; y++)
            {
                var row = image.GetPixelRowSpan(y);
                for (var x = 0; x < width; x++)
                {
                    row[x] = new Rgba32(data[i++], data[i++], data[i++], data[i++]);
                }
            }

            return image;
        }

        private static void Rgb565ToRgba32(ReadOnlySpan<byte> rgb565, ref Rgba32 rgba32)
        {
            var n = (rgb565[0] << 8) | rgb565[1];

            var r = (n & 0b11111_000000_00000) >> 8;
            var g = (n & 0b00000_111111_00000) >> 3;
            var b = (n & 0b00000_000000_11111) << 3;

            rgba32.R = (byte)(r | (r >> 5));
            rgba32.G = (byte)(g | (g >> 6));
            rgba32.B = (byte)(b | (b >> 5));
        }

        private static long ReadLittleInt64(ReadOnlySpan<byte> data)
        {
            var value = 0L;

            for (var i = data.Length - 1; i >= 0; i--)
            {
                value = (value << 8) | data[i];
            }

            return value;
        }

        // Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
        private static Image<Rgba32> ToImage_Dxt1(int width, int height, ReadOnlySpan<byte> data)
        {
            var size = width * height;
            Span<Rgba32> pixels = size <= 1024 ? stackalloc Rgba32[size] : new Rgba32[size];

            Span<Rgba32> colors = stackalloc[]
            {
                default,  // c0
                default,  // c1
                new Rgba32(0x00, 0xff, 0xff, 0xff),  // reserved
                new Rgba32(0x00, 0x00, 0x00, 0x00),  // transparent
            };

            // 8 bytes per block
            for (; !data.IsEmpty; data = data[8..])
            {
                var colorBlock = data[0..8];

                Rgb565ToRgba32(colorBlock[0..2], ref colors[0]);
                Rgb565ToRgba32(colorBlock[2..4], ref colors[1]);

                var colorData = colorBlock[4..8];

                var colorPacked = ReadLittleInt64(colorBlock[4..8]);

                // 16 pixels per block
                for (var i = 0; i < 16; i++, colorPacked >>= 3)
                {
                    var pixel = colors[(int)colorPacked & 0b11];
                }
            }

            var image = new Image<Rgba32>(width, height);

            return Image.LoadPixelData<Rgba32>(pixels, width, height);
        }

        // Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
        private static Image<Rgba32> ToImage_Dxt5(int width, int height, ReadOnlySpan<byte> data)
        {
            Span<Rgba32> pixels = new Rgba32[width * height];

            Span<Rgba32> colors = stackalloc[]
            {
                default,  // c0
                default,  // c1
                new Rgba32(0x00, 0xff, 0xff, 0xff),  // reserved
                new Rgba32(0x00, 0x00, 0x00, 0x00),  // transparent
            };

            Span<byte> alphas = stackalloc byte[]
            {
                0x00,  // a0
                0x00,  // a1
                0x7f,  // reserved
                0x7f,  // reserved
                0x7f,  // reserved
                0x7f,  // reserved
                0x00,  // transparent
                0xff,  // opaque
            };

            // 16 bytes per block
            for (; !data.IsEmpty; data = data[16..])
            {
                var alphaBlock = data[0..8];
                var colorBlock = data[8..16];

                alphas[0] = alphaBlock[0];
                alphas[1] = alphaBlock[1];

                Rgb565ToRgba32(colorBlock[0..2], ref colors[0]);
                Rgb565ToRgba32(colorBlock[2..4], ref colors[1]);

                var alphaData = alphaBlock[2..8];
                var colorData = colorBlock[4..8];

                var alphaPacked = ReadLittleInt64(alphaBlock[2..8]);
                var colorPacked = ReadLittleInt64(colorBlock[4..8]);

                // 16 pixels per block
                for (var i = 0; i < 16; i++, alphaPacked >>= 3, colorPacked >>= 3)
                {
                    var pixel = colors[(int)colorPacked & 0b11];
                    pixel.A = alphas[(int)alphaPacked & 0b111];
                }
            }

            return Image.LoadPixelData<Rgba32>(pixels, width, height);
        }
    }
}
