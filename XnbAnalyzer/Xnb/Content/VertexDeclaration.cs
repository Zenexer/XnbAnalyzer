using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record VertexDeclaration(uint Stride, ImmutableArray<Element> Elements);

    [Serializable]
    public record Element(
        uint Offset,
        ElementFormat ElementFormat,
        ElementUsage ElementUsage,
        uint UsageIndex
    );

    public enum ElementFormat
    {
        Single = 0,
        Vectro2 = 1,
        Vector3 = 2,
        Vector4 = 3,
        Color = 4,
        Byte4 = 5,
        Short2 = 6,
        Short4 = 7,
        NormalizedShort2 = 8,
        NormalizedShort4 = 9,
        HalfVector2 = 10,
        HalfVector4 = 11,
    }

    public enum ElementUsage
    {
        Position = 0,
        Color = 1,
        TextureCoordinate = 2,
        Normal = 3,
        Binormal = 4,
        Tangent = 5,
        BlendIndices = 6,
        BlendWeight = 7,
        Depth = 8,
        Fog = 9,
        PointSize = 10,
        Sample = 11,
        TessellateFactor = 12,
    }
}
