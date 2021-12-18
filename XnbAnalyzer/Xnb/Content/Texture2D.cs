using SixLabors.ImageSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public class Texture2D : Texture
{
    protected override async Task SaveToFolderAsync(string dir, CancellationToken cancellationToken)
    {
        if (MipImages.Length > 1)
        {
            Directory.CreateDirectory(dir);
        }

        var layer = 0;

        foreach (var image in ToImages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var path = MipImages.Length > 1 ? Path.Combine(dir, $"{layer} - {Enum.GetName(SurfaceFormat)} - {image.Width}x{image.Height}.png") : $"{dir}.png";
            await image.SaveAsPngAsync(path, cancellationToken);

            layer++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (SurfaceFormat.IsS3Tc())
        {
            using var tx = File.Create($"{dir}.dds");
            var buffer = new Memory<byte>(new byte[128]);
            WriteDdsHeader(buffer.Span, (uint)MipImages.Length);
            await tx.WriteAsync(buffer, cancellationToken);

            foreach (var image in MipImages)
            {
                await tx.WriteAsync(image.AsMemory(), cancellationToken);
            }

            await tx.FlushAsync(cancellationToken);
        }
    }
}
