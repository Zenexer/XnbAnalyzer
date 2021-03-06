using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
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

    [Reader("System.Collections.Generic.Dictionary`2", "Microsoft.Xna.Framework.Content.DictionaryReader`2")]
    public class DictionaryRedaer<K, V> : Reader<ImmutableDictionary<K, V?>>
        where K : notnull
    {
        public DictionaryRedaer(XnbStreamReader rx) : base(rx) { }

        public override ImmutableDictionary<K, V?> Read()
        {
            var count = Rx.ReadUInt32();
            var dict = new Dictionary<K, V?>();
            for (var i = 0; i < count; i++)
            {
                var k = Rx.ReadObject<K>() ?? throw new XnbFormatException("Dictionary key is null");
                var v = Rx.ReadObject<V>();

                if (dict.ContainsKey(k))
                {
                    throw new XnbFormatException("Dictionary contains duplicate keys");
                }

                dict.Add(k, v);
            }

            return dict.ToImmutableDictionary();
        }
    }
}
