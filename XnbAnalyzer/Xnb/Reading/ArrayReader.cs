using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("T[]", "Microsoft.Xna.Framework.Content.ArrayReader`1")]
public class ArrayReader<T> : AsyncReader<ImmutableArray<T?>>
{
    public ArrayReader(XnbStreamReader rx) : base(rx) { }

    public override async ValueTask<ImmutableArray<T?>> ReadAsync(CancellationToken cancellationToken)
    {
        var count = Rx.ReadUInt32();

        var array = new T?[count];

        for (var i = 0; i < count; i++)
        {
            array[i] = await Rx.ReadObjectAsync<T?>(cancellationToken);
        }

        return array.ToImmutableArray();
    }
}
