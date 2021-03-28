using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record Effect(ImmutableArray<byte> Bytecode);

    [Serializable]
    public record EffectParameter(object Value);
}
