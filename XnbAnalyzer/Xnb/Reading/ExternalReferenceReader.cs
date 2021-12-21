using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("XnbAnalyzer.Xnb.Content.ExternalReference`1", "Microsoft.Xna.Framework.Content.ExternalReferenceReader")]
    public class ExternalReferenceReader<T> : Reader<ExternalReference<T>>
    {
        public ExternalReferenceReader(XnbStreamReader rx) : base(rx) { }

        public override ExternalReference<T> Read() => Rx.ReadExternalReference<T>();
    }
}
