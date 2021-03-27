using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb
{
    public class XnbStreamWriter : BinaryWriter
    {
        public XnbStreamWriter(Stream output) : base(output) { }

        public new void Write7BitEncodedInt(int value) => base.Write7BitEncodedInt(value);

        internal void WriteNullObject()
        {
            throw new NotImplementedException();
        }
    }
}
