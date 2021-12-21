using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("T[]", "Microsoft.Xna.Framework.Content.ArrayReader`1")]
public class ArrayReader<T> : Reader<ImmutableArray<T?>>
{
    public ArrayReader(XnbStreamReader rx) : base(rx) { }

    public override ImmutableArray<T?> Read()
    {
        var count = Rx.ReadUInt32();

        var array = new T?[count];

        for (var i = 0; i < count; i++)
        {
            array[i] = Rx.ReadObject<T?>();
        }

        return array.ToImmutableArray();
    }
}
