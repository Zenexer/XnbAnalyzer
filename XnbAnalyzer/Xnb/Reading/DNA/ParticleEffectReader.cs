﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.Particles.ParticleEffect", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleEffect]]")]
public class ParticleEffectReader : Reader<ParticleEffect>
{
    private readonly ParticleBaseReader<Texture2D> baseReader;

    public ParticleEffectReader(XnbStreamReader rx)
        : base(rx)
    {
        baseReader = new(rx);
    }

    public override ParticleEffect Read()
        => new(baseReader.Read());
}
