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

        using var jsonFile = File.Create(Path.Combine(path, "Sprites.json"));

        await Task.WhenAll(
            JsonSerializer.SerializeAsync(jsonFile, map, new JsonSerializerOptions { WriteIndented = true }, cancellationToken),
            Sprites.Values.First().Texture.ExportAsync(Path.Combine(path, "Texture"), cancellationToken)
        );
    }
}
