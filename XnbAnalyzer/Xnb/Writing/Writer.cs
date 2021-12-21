using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Writing
{
    public interface IWriter { }
    public interface IWriter<T> { }

    public abstract class Writer : IWriter
    {
        protected XnbStreamWriter Tx { get; }

        public Writer(XnbStreamWriter tx)
        {
            Tx = tx;
        }
    }

    public abstract class Writer<T> : Writer, IWriter<T>
    {
        public Writer(XnbStreamWriter tx)
            : base(tx) { }
    }
}
