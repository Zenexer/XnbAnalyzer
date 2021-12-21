using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.VertexDeclaration", "Microsoft.Xna.Framework.Content.VertexDeclarationReader")]
public class VertexDeclarationReader : SyncReader<VertexDeclaration>
{
    public VertexDeclarationReader(XnbStreamReader rx) : base(rx) { }

    public override VertexDeclaration Read()
    {
        var stride = Rx.ReadUInt32();
        var elementCount = Rx.ReadUInt32();
        var elements = new Element[elementCount];

        for (var i = 0; i < elementCount; i++)
        {
            var offset = Rx.ReadUInt32();
            var elementFormat = (ElementFormat)Rx.ReadInt32();
            var elementUsage = (ElementUsage)Rx.ReadInt32();
            var usageIndex = Rx.ReadUInt32();

            elements[i] = new Element(offset, elementFormat, elementUsage, usageIndex);
        }

        return new VertexDeclaration(stride, elements.ToImmutableArray());
    }
}
