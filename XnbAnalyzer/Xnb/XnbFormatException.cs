using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb
{
    public class XnbFormatException : Exception
    {
        public XnbFormatException()
        {
        }

        public XnbFormatException(string? message) : base(message)
        {
        }

        public XnbFormatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected XnbFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
