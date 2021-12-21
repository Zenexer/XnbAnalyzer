using System;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.Particles.ParticleBase`1", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleBase`1]]")]
public class ParticleBaseReader<T> : Reader<ParticleBase<T>>
    where T : Texture
{
    public ParticleBaseReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override ParticleBase<T> Read() => new(
        Rx.ReadString(),
        Rx.ReadObjectNonNull<T>(nameof(ParticleBase<T>.Texture)),
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
        Rx.ReadDirect<TimeSpan>(),
        Rx.ReadDirect<TimeSpan>(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadVector3(),
        Rx.ReadSingle(),
        Rx.ReadDirect<Color>(),
        Rx.ReadDirect<Color>(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadSingle(),
        Rx.ReadBoolean()
    );
}

