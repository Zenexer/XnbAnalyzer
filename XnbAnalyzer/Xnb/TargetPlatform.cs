using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb
{
    public enum TargetPlatform : byte
    {
        Unknown = 0,
        MicrosoftWindows = (byte)'w',
        WindowsPhone7 = (byte)'m',
        Xbox360 = (byte)'x',
    }
}
