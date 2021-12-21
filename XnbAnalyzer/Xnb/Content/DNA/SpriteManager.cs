using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA;

[Serializable]
public record class SpriteManager(ImmutableDictionary<string, Sprite> Sprites) : IExportable
{
    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(path);

        var map = new Dictionary<string, Rectangle>();

        foreach (var kv in Sprites)
        {
            map[kv.Key] = kv.Value.SourceRectangle;
        }

        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });

        await Task.WhenAll(
            File.WriteAllTextAsync(Path.Combine(path, "Sprites.json"), json, cancellationToken),
            Sprites.Values.First().Texture.ExportAsync(Path.Combine(path, "Texture"), cancellationToken)
        );
    }
}
