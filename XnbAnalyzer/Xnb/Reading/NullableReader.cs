using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("System.Nullable`1", "Microsoft.Xna.Framework.Content.NullableReader`1")]
    public class NullableReader<T> : AsyncReader<T?>
        where T : struct
    {
        public NullableReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override async ValueTask<T?> ReadAsync(CancellationToken cancellationToken)
        {
            var hasValue = Rx.ReadBoolean();
            return hasValue ? await Rx.ReadDirectAsync<T>(cancellationToken) : null;
        }
    }
}
