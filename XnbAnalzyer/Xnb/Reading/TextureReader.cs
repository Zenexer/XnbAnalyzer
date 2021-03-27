using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalzyer.Xnb.Content;

namespace XnbAnalzyer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.Texture", "Microsoft.Xna.Framework.Content.TextureReader")]
    public class TextureReader : Reader<Texture>
    {
        public TextureReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override Texture Read() => throw new Exception("Logic issue");
    }
}
