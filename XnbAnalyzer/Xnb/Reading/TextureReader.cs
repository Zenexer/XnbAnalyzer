using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Graphics.Texture", "Microsoft.Xna.Framework.Content.TextureReader")]
public class TextureReader : SyncReader<Texture>
{
    public TextureReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override Texture Read() => throw new Exception("Logic issue");
}
