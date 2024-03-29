﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb;

public enum SurfaceFormat
{
    Color = 0,
    Bgr565 = 1,
    Bgra5551 = 2,
    Bgra4444 = 3,
    Dxt1 = 4,
    Dxt3 = 5,
    Dxt5 = 6,
    NormalizedByte2 = 7,
    NormalizedByte4 = 8,
    Rgba1010102 = 9,
    Rg32 = 10,
    Rgba64 = 11,
    Alpha8 = 12,
    Single = 13,
    Vector2 = 14,
    Vector4 = 15,
    HalfSingle = 16,
    HalfVector2 = 17,
    HalfVector4 = 18,
    HdrBlendable = 19,
}

public static class SurfaceFormatExtension
{
    public static bool IsS3Tc(this SurfaceFormat format) => format switch
    {
        SurfaceFormat.Dxt1 => true,
        SurfaceFormat.Dxt3 => true,
        SurfaceFormat.Dxt5 => true,
        _ => false,
    };
}
