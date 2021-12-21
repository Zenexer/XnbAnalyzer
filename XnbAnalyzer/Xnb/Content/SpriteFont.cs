using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public record SpriteFont(
    Texture2D Texture,
    SpriteFontInfo Info
) : IExportable
{
    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(path);

        using var tx = File.Create(Path.Combine(path, "SpriteFont.json"));

        await Task.WhenAll(
            JsonSerializer.SerializeAsync(tx, Info, new JsonSerializerOptions() { WriteIndented = true }, cancellationToken),
            Texture.ExportAsync(Path.Combine(path, "Texture"), cancellationToken)
        );
    }
}

[Serializable]
public record SpriteFontInfo(
    ImmutableList<Rectangle> Glyphs,
    ImmutableList<Rectangle> Cropping,
    ImmutableList<char> CharacterMap,
    int VerticalLineSpacing,
    float HorizontalSpacing,
    ImmutableList<Vector3> Kerning,
    char? DefaultCharacter
);
