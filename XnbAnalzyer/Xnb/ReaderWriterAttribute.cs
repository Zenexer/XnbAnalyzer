using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb
{
    public abstract class ReaderWriterAttribute : Attribute
    {
        public string TargetType { get; }
        public string TypeReaderName { get; }
        public bool IsPrimitive { get; }

        public ReaderWriterAttribute(string targetType, string typeReaderName)
        {
            TargetType = targetType;
            TypeReaderName = typeReaderName;
        }
    }
}
