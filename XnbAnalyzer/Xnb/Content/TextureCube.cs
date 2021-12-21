using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public class TextureCube : Texture
{
    public uint Size { get; init; }

    protected override async Task SaveToFolderAsync(string dir, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(dir);

        var layer = 0;
        var sides = new[] { "+x", "-x", "+y", "-y", "+z", "-z" };

        foreach (var image in ToImages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = Path.Combine(dir, $"{layer / 6} - Side {sides[layer % 6]}- {Enum.GetName(SurfaceFormat)} - {image.Width}px.png");
            await image.SaveAsPngAsync(path, cancellationToken);

            layer++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (SurfaceFormat.IsS3Tc())
        {
            var buffer = new Memory<byte>(new byte[128]);
            WriteDdsHeader(buffer.Span, (uint)MipImages.Length / 6);

            for (var s = 0; s < 6; s++)
            {
                using var tx = File.Create(Path.Combine(dir, $"Side {sides[s]}.dds"));
                await tx.WriteAsync(buffer, cancellationToken);

                for (var i = 0; i < MipImages.Length / 6; i++)
                {
                    await tx.WriteAsync(MipImages[s * 6 + i].AsMemory(), cancellationToken);
                }

                await tx.FlushAsync(cancellationToken);
            }
        }
    }
}
