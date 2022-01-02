using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA;

// In reality, this extends Effect, but that would be inconvenient for our purposes since it's an external reference
[Serializable]
public record DNAEffect(
    ExternalReference<Effect> EffectBytecodeReference,
    ImmutableDictionary<string, ExternalReference<Texture>> TextureReferences,
    ImmutableDictionary<string, EffectParameter> AdditionalParameters
) : IExportable
{
    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        using var tx = File.Create($"{path}.json");
        await JsonSerializer.SerializeAsync(tx, this, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }
}
