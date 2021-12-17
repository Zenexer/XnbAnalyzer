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
            SurfaceFormat.Dxt3 => ToImage_Dxt3(width, height, data),
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

                var name = $"{layer} - {Enum.GetName(SurfaceFormat)} - {image.Width}x{image.Height}.png";
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

        private static int ReadRgb565(ReadOnlySpan<byte> bytes) => (bytes[0] << 8) | bytes[1];

        private static void Rgb565ToRgba32(int rgb565, ref Rgba32 rgba32, byte alpha = 0xff)
        {
            var r = (rgb565 >> 11) & 0b11111;
            var g = (rgb565 >> 5) & 0b111111;
            var b = rgb565 & 0b11111;

            r = (r << 3) | r >> 2;
            g = (g << 2) | g >> 3;
            b = (b << 3) | b >> 2;

            rgba32.R = (byte)r;
            rgba32.G = (byte)g;
            rgba32.B = (byte)b;
            rgba32.A = alpha;
        }

        private static void ReadPackedData2bpp(ReadOnlySpan<byte> packed, Span<byte> unpacked)
        {
            var i = 0;
            foreach (var b in packed)
            {
                unpacked[i++] = unchecked((byte)((b >> 6) & 0b11));
                unpacked[i++] = unchecked((byte)((b >> 4) & 0b11));
                unpacked[i++] = unchecked((byte)((b >> 2) & 0b11));
                unpacked[i++] = unchecked((byte)((b >> 0) & 0b11));
            }
        }

        private static void ReadPackedData3bpp(ReadOnlySpan<byte> packed, Span<byte> unpacked)
        {
            if (packed.Length % 3 != 0)
            {
                throw new ArgumentException($"{nameof(packed)}'s length must be a multiple of 3", nameof(packed));
            }


            var u = 0;
            for (var p = 0; p < packed.Length; )
            {
                // 0001_1122 2333_4445 5566_6777
                var tmp = unchecked((packed[p++] << 16) | (packed[p++] << 8) | packed[p++]);

                for (var shift = 21; shift >= 0; shift -= 3)
                {
                    unpacked[u++] = unchecked((byte)((tmp >> shift) & 0b111));
                }
            }
        }

        // Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
        // The reserved values need to be calculated per https://en.wikipedia.org/wiki/S3_Texture_Compression
        private static Image<Rgba32> ToImage_Dxt1(int width, int height, ReadOnlySpan<byte> data)
        {
            var size = width * height;
            Span<Rgba32> pixels = size <= 1024 ? stackalloc Rgba32[size] : new Rgba32[size];
            Span<byte> colorIndices = stackalloc byte[16];
            Span<Rgba32> colors = stackalloc Rgba32[4];

            var blockW = (int)Math.Ceiling(width / 4.0);
            var blockH = (int)Math.Ceiling(height / 4.0);

            // 8 bytes per block
            for (var block = 0; !data.IsEmpty; block++, data = data[8..])
            {
                var blockX = block % blockW;
                var blockY = block / blockW;
                var startX = blockX * 4;
                var startY = blockY * 4;

                var colorBlock = data[0..8];

                int c0, c1, c2, c3;
                byte c3alpha;

                c0 = ReadRgb565(colorBlock[0..2]);
                c1 = ReadRgb565(colorBlock[2..4]);

                if (c0 > c1)
                {
                    c2 = (c0 * 2 + c1 * 1) / 3;
                    c3 = (c0 * 1 + c1 * 2) / 3;
                    c3alpha = 0xff;
                }
                else
                {
                    c2 = (c0 + c1) / 2;
                    c3 = 0;
                    c3alpha = 0;
                }

                Rgb565ToRgba32(c0, ref colors[0]);
                Rgb565ToRgba32(c1, ref colors[1]);
                Rgb565ToRgba32(c2, ref colors[2]);
                Rgb565ToRgba32(c3, ref colors[3], c3alpha);

                var colorData = colorBlock[4..8];

                ReadPackedData2bpp(colorBlock[4..8], colorIndices);

                // 16 pixels per block
                for (var i = 0; i < 16; i++)
                {
                    var x = startX + i % 4;
                    var y = startY + i / 4;
                    if (x >= width || y >= height)
                    {
                        continue;
                    }

                    var pixelIndex = y * width + x;
                    var colorIndex = colorIndices[i];

                    pixels[pixelIndex] = colors[colorIndex];
                }
            }

            var image = new Image<Rgba32>(width, height);

            return Image.LoadPixelData<Rgba32>(pixels, width, height);
        }

        // Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
        private static Image<Rgba32> ToImage_Dxt3(int width, int height, ReadOnlySpan<byte> data)
        {
            Span<Rgba32> pixels = new Rgba32[width * height];
            Span<byte> colorIndices = stackalloc byte[16];
            Span<Rgba32> colors = stackalloc Rgba32[4];

            var blockW = (int)Math.Ceiling(width / 4.0);
            var blockH = (int)Math.Ceiling(height / 4.0);

            // 16 bytes per block
            for (var block = 0; !data.IsEmpty; block++, data = data[16..])
            {
                var blockX = block % blockW;
                var blockY = block / blockW;
                var startX = blockX * 4;
                var startY = blockY * 4;

                var alphaBlock = data[0..8];
                var colorBlock = data[8..16];

                var c0 = ReadRgb565(colorBlock[0..2]);
                var c1 = ReadRgb565(colorBlock[2..4]);
                var c2 = (c0 * 2 + c1 * 1) / 3;
                var c3 = (c0 * 1 + c1 * 2) / 3;

                Rgb565ToRgba32(c0, ref colors[0]);
                Rgb565ToRgba32(c1, ref colors[1]);
                Rgb565ToRgba32(c2, ref colors[2]);
                Rgb565ToRgba32(c3, ref colors[3]);

                var colorData = colorBlock[4..8];

                ReadPackedData2bpp(colorBlock[4..8], colorIndices);

                // 16 pixels per block
                for (var i = 0; i < 16; i++)
                {
                    var x = startX + 4 - i % 4;
                    var y = startY + i / 4;
                    if (x >= width || y >= height)
                    {
                        continue;
                    }

                    var pixelIndex = y * width + x;
                    var colorIndex = colorIndices[i];
                    var color = colors[colorIndex];
                    var alpha = (int)alphaBlock[7 - i / 2];

                    if (i % 2 == 1)
                    {
                        alpha >>= 4;
                    }

                    alpha &= 0b1111;
                    alpha |= alpha << 4;

                    pixels[pixelIndex] = color with { A = (byte)alpha };
                }
            }

            return Image.LoadPixelData<Rgba32>(pixels, width, height);
        }

        // Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
        private static Image<Rgba32> ToImage_Dxt5(int width, int height, ReadOnlySpan<byte> data)
        {
            Span<Rgba32> pixels = new Rgba32[width * height];
            Span<byte> colorIndices = stackalloc byte[16];
            Span<byte> alphaIndices = stackalloc byte[16];

            Span<Rgba32> colors = stackalloc[]
            {
                default,  // c0
                default,  // c1
                new Rgba32(0x00, 0xff, 0xff, 0xff),  // reserved
                new Rgba32(0x00, 0x00, 0x00, 0x00),  // transparent
            };

            Span<byte> alphas = stackalloc byte[8];

            var blockW = (int)Math.Ceiling(width / 4.0);
            var blockH = (int)Math.Ceiling(height / 4.0);

            // 16 bytes per block
            for (var block = 0; !data.IsEmpty; block++, data = data[16..])
            {
                var blockX = block % blockW;
                var blockY = block / blockW;
                var startX = blockX * 4;
                var startY = blockY * 4;

                var alphaBlock = data[0..8];
                var colorBlock = data[8..16];

                alphas[0] = alphaBlock[0];
                alphas[1] = alphaBlock[1];

                if (alphas[0] > alphas[1])
                {
                    for (var a = 0; a < 6; a++)
                    {
                        alphas[a + 2] = (byte)(((6 - a) * alphas[0] + (a + 1) * alphas[1]) / 7);
                    }
                }
                else
                {
                    for (var a = 0; a < 4; a++)
                    {
                        alphas[a + 2] = (byte)(((4 - a) * alphas[0] + (a + 1) * alphas[1]) / 5);
                        alphas[6] = 0x00;
                        alphas[7] = 0xff;
                    }
                }

                int c0, c1, c2, c3;
                byte c3alpha;

                c0 = ReadRgb565(colorBlock[0..2]);
                c1 = ReadRgb565(colorBlock[2..4]);

                if (c0 > c1)
                {
                    c2 = (c0 * 2 + c1 * 1) / 3;
                    c3 = (c0 * 1 + c1 * 2) / 3;
                    c3alpha = 0xff;
                }
                else
                {
                    c2 = (c0 + c1) / 2;
                    c3 = 0;
                    c3alpha = 0;
                }

                Rgb565ToRgba32(c0, ref colors[0]);
                Rgb565ToRgba32(c1, ref colors[1]);
                Rgb565ToRgba32(c2, ref colors[2]);
                Rgb565ToRgba32(c3, ref colors[3], c3alpha);

                var alphaData = alphaBlock[2..8];
                var colorData = colorBlock[4..8];

                ReadPackedData3bpp(alphaBlock[2..8], alphaIndices);
                ReadPackedData2bpp(colorBlock[4..8], colorIndices);

                // 16 pixels per block
                for (var i = 0; i < 16; i++)
                {
                    var x = startX + i % 4;
                    var y = startY + i / 4;
                    if (x >= width || y >= height)
                    {
                        continue;
                    }

                    var pixelIndex = y * width + x;
                    var colorIndex = colorIndices[i];
                    var alphaIndex = alphaIndices[i];

                    pixels[pixelIndex] = colors[colorIndex] with { A = alphas[alphaIndex] };
                }
            }

            return Image.LoadPixelData<Rgba32>(pixels, width, height);
        }
    }
}
