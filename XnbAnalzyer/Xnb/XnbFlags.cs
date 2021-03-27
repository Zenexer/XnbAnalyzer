using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb
{
    [Flags]
    public enum XnbFlags : byte
    {
        None = 0,
        HiDefProfile = 0x01,
        CompressedLZX = 0x80,
    }
}
