using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    public abstract class Reader
    {
        protected XnbStreamReader Rx { get; }

        public Reader(XnbStreamReader rx)
        {
            Rx = rx;
        }

        public abstract object? ReadObject();
    }

    public abstract class Reader<T> : Reader
    {
        public Reader(XnbStreamReader rx) : base(rx) { }

        public abstract T Read();

        public override object? ReadObject() => Read();
    }
}
