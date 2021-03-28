using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Writing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WriterAttribute : ReaderWriterAttribute
    {
        public WriterAttribute(string targetType, string typeReaderName)
            : base(targetType, typeReaderName) { }
    }
}
