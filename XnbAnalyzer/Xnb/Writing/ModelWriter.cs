using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Writing
{
    public class ModelWriter : Writer<Model>
    {
        public ModelWriter(XnbStreamWriter tx) : base(tx)
        {
        }
    }
}
