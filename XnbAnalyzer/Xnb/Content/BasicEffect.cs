using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record BasicEffect(
        ExternalReference<Texture> Texture,
        Vector3 DiffuseColor,
        Vector3 EmissiveColor,
        Vector3 SpecularColor,
        float SpecularPower,
        float Alpha,
        bool VertexColorEnabled
    );
}
