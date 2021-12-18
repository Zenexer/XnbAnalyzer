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

namespace XnbAnalyzer.Xnb.Content;

// Best reference I've found: https://github.com/divVerent/s2tc/wiki/FileFormats
// The reserved values need to be calculated per https://en.wikipedia.org/wiki/S3_Texture_Compression
// Also useful: https://www.khronos.org/opengl/wiki/S3_Texture_Compression
public abstract class Texture : IExportable
{
    public SurfaceFormat SurfaceFormat { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
    public ImmutableArray<ImmutableArray<byte>> MipImages { get; init; }

    public Texture()
    {
        if (!BitConverter.IsLittleEndian)
        {
            throw new NotSupportedException($"{nameof(Texture2D)} only supports little endian platforms");
        }
    }

    public async Task ExportAsync(string path, CancellationToken cancellationToken)
        => await SaveToFolderAsync(path, cancellationToken);

    protected abstract Task SaveToFolderAsync(string dir, CancellationToken cancellationToken);

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
    protected void WriteDdsHeader(Span<byte> bytes, uint mips)
    {
        var surfaceFormat = SurfaceFormat switch
        {
            SurfaceFormat.Dxt1 => 0x31545844u,  // "DXT1"
            SurfaceFormat.Dxt3 => 0x33545844u,  // "DXT3"
            SurfaceFormat.Dxt5 => 0x35545844u,  // "DXT5"
            _ => throw new InvalidOperationException(),
        };

        var uints = MemoryMarshal.Cast<byte, uint>(bytes);

        uints[0 >> 2] = 0x20534444;  // "DDS "
        uints[4 >> 2] = 0x7c;
        uints[8 >> 2] = 0xa1007;
        uints[12 >> 2] = Height;
        uints[16 >> 2] = Width;
        uints[20 >> 2] = (Width + 3) / 4 * (Height + 3) / 4 * (SurfaceFormat == SurfaceFormat.Dxt1 ? 8u : 16u);
        uints[24 >> 2] = 0;
        uints[28 >> 2] = mips;
        bytes[32..76].Fill(0);
        uints[76 >> 2] = 0x20;
        uints[80 >> 2] = 0x5;
        uints[84 >> 2] = surfaceFormat;
        bytes[88..108].Fill(0);
        uints[27] = 0x00401008;
        bytes[112..128].Fill(0);
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

    private static int ReadRgb565(ReadOnlySpan<byte> bytes) => bytes[0] | (bytes[1] << 8);

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

    private static void ReadPackedData2bpp2(ReadOnlySpan<byte> packed, Span<byte> unpacked)
    {
        var i = 0;
        foreach (var b in packed)
        {
            // 4-byte (32-bit) little-endian words; least significant bits in each word 
            // 3322_1100 7766_5544, ...
            unpacked[i++] = unchecked((byte)((b >> 0) & 0b11));
            unpacked[i++] = unchecked((byte)((b >> 2) & 0b11));
            unpacked[i++] = unchecked((byte)((b >> 4) & 0b11));
            unpacked[i++] = unchecked((byte)((b >> 6) & 0b11));
        }
    }

    private static void ReadPackedData2bpp(ReadOnlySpan<byte> packed, Span<byte> unpacked)
    {
        if (packed.Length % 4 != 0)
        {
            throw new ArgumentException($"{nameof(packed)}'s length must be a multiple of 4", nameof(packed));
        }

        var u = 0;
        for (var p = 0; p < packed.Length;)
        {
            // 4-byte (32-bit) little-endian words; least-significant bits in each word are the first pixels of that word
            var tmp = unchecked(
                (long)packed[p++]
                | ((long)packed[p++] << 8)
                | ((long)packed[p++] << 16)
                | ((long)packed[p++] << 24)
            );

            for (var i = 0; i < 16; i++, tmp >>= 2)
            {
                unpacked[u++] = unchecked((byte)(tmp & 0b11));
            }
        }
    }

    private static void ReadPackedData3bpp(ReadOnlySpan<byte> packed, Span<byte> unpacked)
    {
        if (packed.Length % 6 != 0)
        {
            throw new ArgumentException($"{nameof(packed)}'s length must be a multiple of 6", nameof(packed));
        }

        var u = 0;
        for (var p = 0; p < packed.Length;)
        {
            // 6-byte (48-bit) little-endian words; least-significant bits in each word are the first pixels of that word
            // ...999_9888 7776_6655 5444_3332 2211_1000, ...
            var tmp = unchecked(
                (long)packed[p++]
                | ((long)packed[p++] << 8)
                | ((long)packed[p++] << 16)
                | ((long)packed[p++] << 24)
                | ((long)packed[p++] << 32)
                | ((long)packed[p++] << 40)
            );

            for (var i = 0; i < 16; i++, tmp >>= 3)
            {
                unpacked[u++] = unchecked((byte)(tmp & 0b111));
            }
        }
    }

    private static Image<Rgba32> ToImage_Dxt1(int width, int height, ReadOnlySpan<byte> data)
    {
        var size = width * height;
        Span<Rgba32> pixels = size <= 1024 ? stackalloc Rgba32[size] : new Rgba32[size];
        Span<byte> colorIndices = stackalloc byte[16];
        Span<Rgba32> c = stackalloc Rgba32[4];
        c.Fill(new Rgba32(0, 0, 0));

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

            var c0i = ReadRgb565(colorBlock[0..2]);
            var c1i = ReadRgb565(colorBlock[2..4]);
            Rgb565ToRgba32(c0i, ref c[0]);
            Rgb565ToRgba32(c1i, ref c[1]);

            if (c0i > c1i)
            {
                c[2].R = (byte)((c[0].R * 2 + c[1].R * 1) / 3);
                c[2].G = (byte)((c[0].G * 2 + c[1].G * 1) / 3);
                c[2].B = (byte)((c[0].B * 2 + c[1].B * 1) / 3);

                c[3].R = (byte)((c[0].R * 1 + c[1].R * 2) / 3);
                c[3].G = (byte)((c[0].G * 1 + c[1].G * 2) / 3);
                c[3].B = (byte)((c[0].B * 1 + c[1].B * 2) / 3);
                c[3].A = 0xff;
            }
            else
            {
                c[2].R = (byte)((c[0].R + c[1].R) / 2);
                c[2].G = (byte)((c[0].G + c[1].G) / 2);
                c[2].B = (byte)((c[0].B + c[1].B) / 2);

                c[3].R = 0;
                c[3].G = 0;
                c[3].B = 0;
                c[3].A = 0xff;  // For extended DXT1, which supports 1-bit alpha, this should be set to 0.  Not sure which XNA uses.
            }

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

                pixels[pixelIndex] = c[colorIndex];
            }
        }

        var image = new Image<Rgba32>(width, height);

        return Image.LoadPixelData<Rgba32>(pixels, width, height);
    }

    private static Image<Rgba32> ToImage_Dxt3(int width, int height, ReadOnlySpan<byte> data)
    {
        Span<Rgba32> pixels = new Rgba32[width * height];
        Span<byte> colorIndices = stackalloc byte[16];
        Span<Rgba32> c = stackalloc Rgba32[4];
        c.Fill(new Rgba32(0, 0, 0));

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

            var c0i = ReadRgb565(colorBlock[0..2]);
            var c1i = ReadRgb565(colorBlock[2..4]);
            Rgb565ToRgba32(c0i, ref c[0]);
            Rgb565ToRgba32(c1i, ref c[1]);

            c[2].R = (byte)((c[0].R * 2 + c[1].R * 1) / 3);
            c[2].G = (byte)((c[0].G * 2 + c[1].G * 1) / 3);
            c[2].B = (byte)((c[0].B * 2 + c[1].B * 1) / 3);

            c[3].R = (byte)((c[0].R * 1 + c[1].R * 2) / 3);
            c[3].G = (byte)((c[0].G * 1 + c[1].G * 2) / 3);
            c[3].B = (byte)((c[0].B * 1 + c[1].B * 2) / 3);

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
                var color = c[colorIndex];
                var alpha = (int)alphaBlock[i / 2];

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

    private static Image<Rgba32> ToImage_Dxt5(int width, int height, ReadOnlySpan<byte> data)
    {
        Span<Rgba32> pixels = new Rgba32[width * height];
        Span<byte> colorIndices = stackalloc byte[16];
        Span<byte> alphaIndices = stackalloc byte[16];
        Span<byte> alphas = stackalloc byte[8];
        Span<Rgba32> c = stackalloc Rgba32[4];
        c.Fill(new Rgba32(0, 0, 0));

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

            var c0i = ReadRgb565(colorBlock[0..2]);
            var c1i = ReadRgb565(colorBlock[2..4]);
            Rgb565ToRgba32(c0i, ref c[0]);
            Rgb565ToRgba32(c1i, ref c[1]);

            c[2].R = (byte)((c[0].R * 2 + c[1].R * 1) / 3);
            c[2].G = (byte)((c[0].G * 2 + c[1].G * 1) / 3);
            c[2].B = (byte)((c[0].B * 2 + c[1].B * 1) / 3);

            c[3].R = (byte)((c[0].R * 1 + c[1].R * 2) / 3);
            c[3].G = (byte)((c[0].G * 1 + c[1].G * 2) / 3);
            c[3].B = (byte)((c[0].B * 1 + c[1].B * 2) / 3);

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

                pixels[pixelIndex] = c[colorIndex] with { A = alphas[alphaIndex] };
            }
        }

        return Image.LoadPixelData<Rgba32>(pixels, width, height);
    }
}
