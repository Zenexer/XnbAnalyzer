using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Writing
{
    public abstract class Writer
    {
        protected XnbStreamWriter Tx { get; }

        public Writer(XnbStreamWriter tx)
        {
            Tx = tx;
        }
    }

    public abstract class Writer<T> : Writer
    {
        public Writer(XnbStreamWriter tx)
            : base(tx) { }
    }
}
