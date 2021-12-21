using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA;

[Serializable]
public record class ParticleEffect : ParticleBase<Texture2D>
{
    public ParticleEffect(ParticleBase<Texture2D> original) : base(original)
    {
    }
}
