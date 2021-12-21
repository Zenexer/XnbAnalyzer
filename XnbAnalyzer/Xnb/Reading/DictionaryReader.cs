using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("System.Collections.Generic.Dictionary`2", "Microsoft.Xna.Framework.Content.DictionaryReader`2")]
public class DictionaryRedaer<K, V> : AsyncReader<ImmutableDictionary<K, V?>>
    where K : notnull
{
    public DictionaryRedaer(XnbStreamReader rx) : base(rx) { }

    public override async ValueTask<ImmutableDictionary<K, V?>> ReadAsync(CancellationToken cancellationToken)
    {
        var count = Rx.ReadUInt32();
        var dict = new Dictionary<K, V?>();
        for (var i = 0; i < count; i++)
        {
            var k = await Rx.ReadObjectAsync<K>(cancellationToken) ?? throw new XnbFormatException("Dictionary key is null");
            var v = await Rx.ReadObjectAsync<V>(cancellationToken);

            if (dict.ContainsKey(k))
            {
                throw new XnbFormatException("Dictionary contains duplicate keys");
            }

            dict.Add(k, v);
        }

        return dict.ToImmutableDictionary();
    }
}
