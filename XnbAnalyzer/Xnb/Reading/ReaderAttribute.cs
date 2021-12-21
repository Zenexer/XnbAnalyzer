using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ReaderAttribute : ReaderWriterAttribute
    {
        public ReaderAttribute(string targetType, string typeReaderName)
            : base(targetType, typeReaderName) { }
    }
}
