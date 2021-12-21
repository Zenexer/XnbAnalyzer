using System;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.Particles.ParticleBase`1", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleBase`1]]")]
public class ParticleBaseReader<T> : AsyncReader<ParticleBase<T>>
    where T : Texture
{
    public ParticleBaseReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override async ValueTask<ParticleBase<T>> ReadAsync(CancellationToken cancellationToken)
    {
        var texturePath = await Rx.ReadObjectNonNullAsync<string>(nameof(ParticleBase<T>.TexturePath), cancellationToken);
        return new(
            texturePath,
            await Rx.ReadObjectNonNullAsync<T>(nameof(ParticleBase<T>.Texture), cancellationToken),
            Rx.ReadInt32(),
            Rx.ReadInt32(),
            Rx.ReadInt32(),
            Rx.ReadInt32(),
            Rx.ReadEnum<ParticleBlendMode>(false, true),
            Rx.ReadEnum<ParticleTechnique>(false, true),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadBoolean(),
            Rx.ReadSingle(),
            Rx.ReadBoolean(),
            Rx.ReadBoolean(),
            await Rx.ReadDirectAsync<TimeSpan>(cancellationToken),
            await Rx.ReadDirectAsync<TimeSpan>(cancellationToken),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadVector3(),
            Rx.ReadSingle(),
            await Rx.ReadDirectAsync<Color>(cancellationToken),
            await Rx.ReadDirectAsync<Color>(cancellationToken),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadSingle(),
            Rx.ReadBoolean()
        );
    }
}

