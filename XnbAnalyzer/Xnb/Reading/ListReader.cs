using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("System.Collections.Generic.List`1", "Microsoft.Xna.Framework.Content.ListReader`1")]
public class ListReader<T> : AsyncReader<ImmutableList<T?>>
{
    public ListReader(XnbStreamReader rx) : base(rx) { }

    public override async ValueTask<ImmutableList<T?>> ReadAsync(CancellationToken cancellationToken)
    {
        var count = Rx.ReadUInt32();

        var array = new T?[count];

        for (var i = 0; i < count; i++)
        {
            array[i] = await Rx.ReadObjectAsync<T?>(cancellationToken);
        }

        return array.ToImmutableList();
    }
}
