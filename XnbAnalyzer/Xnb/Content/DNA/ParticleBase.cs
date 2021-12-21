using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA;

public record class ParticleBase<T>(
    string TexturePath,
    T Texture,
    int NumTilesWide,
    int NumTilesHigh,
    int FirstTileToInclude,
    int LastTileToInclude,
    ParticleBlendMode BlendMode,
    ParticleTechnique Technique,
    float DistortionScale,
    float DistortionAmplitude,
    bool RandomizeRotations,
    float ParticlesPerSecond,
    bool LocalSpace,
    bool FadeOut,
    TimeSpan EmissionTime,
    TimeSpan ParticleLifeTime,
    float LifetimeVariation,
    float EmitterVelocitySensitivity,
    float HorizontalVelocityMin,
    float HorizontalVelocityMax,
    float VerticalVelocityMin,
    float VerticalVelocityMax,
    Vector3 Gravity,
    float VelocityEnd,
    Color ColorMin,
    Color ColorMax,
    float RotateSpeedMin,
    float RotateSpeedMax,
    float StartSizeMin,
    float StartSizeMax,
    float EndSizeMin,
    float EndSizeMax,
    bool DieAfterEmission
)
    : IExportable
    where T : Texture
{
    public Vector2 TileSize => new(1 / NumTilesWide, 1 / NumTilesHigh);

    public async Task ExportAsync(string path, CancellationToken cancellationToken)
    {
        // TODO: Export metadata

        var texturePath = Path.Combine(path, "Texture");
        Directory.CreateDirectory(texturePath);
        await Texture.ExportAsync(texturePath, cancellationToken);
    }
}
