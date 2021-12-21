using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record SharedResourceRef(int Id)
    {
        public bool HasValue => Id != 0;
    }

    [Serializable]
    public record SharedResourceRef<T>(int Id) : SharedResourceRef(Id);
}
