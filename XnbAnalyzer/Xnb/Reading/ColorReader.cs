using System;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("Microsoft.Xna.Framework.Color", "Microsoft.Xna.Framework.Content.ColorReader")]
public class ColorReader : Reader<Color>
{
    public ColorReader(XnbStreamReader rx) : base(rx)
    {
    }

    public override Color Read()  => new(
        Rx.ReadByte(),
        Rx.ReadByte(),
        Rx.ReadByte(),
        Rx.ReadByte()
    );
}

