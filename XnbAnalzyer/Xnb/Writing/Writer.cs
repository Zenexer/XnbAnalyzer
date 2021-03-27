using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb.Writing
{
    public abstract class Writer
    {
        protected XnbStreamWriter Tx { get; }

        public Writer(XnbStreamWriter tx)
        {
            Tx = tx;
        }

        public abstract object? ReadObject();
    }

    public abstract class Writer<T> : Writer
    {
        public Writer(XnbStreamWriter tx)
            : base(tx) { }

        public abstract T Read();

        public override object? ReadObject() => Read();
    }
}
