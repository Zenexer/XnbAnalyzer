using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record VertexBuffer(VertexDeclaration Declaration, uint Count, ImmutableArray<byte> Data);
}
