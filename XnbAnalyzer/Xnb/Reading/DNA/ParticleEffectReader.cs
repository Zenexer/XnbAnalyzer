using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.Particles.ParticleEffectASDF", "Microsoft.Xna.Framework.Content.ReflectiveReader`1[[DNA.Drawing.Particles.ParticleEffect]]")]
public class ParticleEffectReader : Reader<ParticleEffect>
{
    public ParticleEffectReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override ParticleEffect Read()
    {
        throw new NotImplementedException();
    }
}
