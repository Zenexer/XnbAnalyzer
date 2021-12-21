using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Writing;

[Writer("Microsoft.Xna.Framework.BasicEffect", "Microsoft.Xna.Framework.Content.BasicEffectWriter")]
public class BasicEffectWriter : Writer<BasicEffect>
{
    public BasicEffectWriter(XnbStreamWriter tx) : base(tx)
    {
    }
}
