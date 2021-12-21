using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("System.Nullable`1", "Microsoft.Xna.Framework.Content.NullableReader`1")]
    public class NullableReader<T> : Reader<T?>
        where T : struct
    {
        public NullableReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override T? Read()
        {
            var hasValue = Rx.ReadBoolean();
            return hasValue ? Rx.ReadDirect<T>() : null;
        }
    }
}
