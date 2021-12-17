using System;
using System.Numerics;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public record BoundingBox(Vector3 Min, Vector3 Max);
