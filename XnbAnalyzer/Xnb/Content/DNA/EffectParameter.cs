using System;
using System.Collections.Immutable;
using System.Numerics;

namespace XnbAnalyzer.Xnb.Content.DNA;

public abstract class EffectParameter
{
    public abstract EffectValueTypes Type { get; }
    public bool IsArray { get; }

    public EffectParameter(bool isArray)
    {
        IsArray = isArray;
    }
}

public abstract class EffectParameter<T> : EffectParameter
    where T : struct
{
    public ImmutableArray<T> Value { get; }

    public EffectParameter(bool isArray, T[] value)
        : base(isArray)
    {
        Value = value.ToImmutableArray();
    }
}

public class Int32EffectParameter : EffectParameter<int>
{
    public override EffectValueTypes Type => EffectValueTypes.Int32Value;

    public Int32EffectParameter(bool isArray, int[] value) : base(isArray, value) { }
}

public class BooleanEffectParameter : EffectParameter<bool>
{
    public override EffectValueTypes Type => EffectValueTypes.BooleanValue;

    public BooleanEffectParameter(bool isArray, bool[] value) : base(isArray, value) { }
}

public class SingleEffectParameter : EffectParameter<float>
{
    public override EffectValueTypes Type => EffectValueTypes.SingleValue;

    public SingleEffectParameter(bool isArray, float[] value) : base(isArray, value) { }
}

public class Vector2EffectParameter : EffectParameter<Vector2>
{
    public override EffectValueTypes Type => EffectValueTypes.Vector2Value;

    public Vector2EffectParameter(bool isArray, Vector2[] value) : base(isArray, value) { }
}

public class Vector3EffectParameter : EffectParameter<Vector3>
{
    public override EffectValueTypes Type => EffectValueTypes.Vector3Value;

    public Vector3EffectParameter(bool isArray, Vector3[] value) : base(isArray, value) { }
}

public class Vector4EffectParameter : EffectParameter<Vector4>
{
    public override EffectValueTypes Type => EffectValueTypes.Vector4Value;

    public Vector4EffectParameter(bool isArray, Vector4[] value) : base(isArray, value) { }
}

public class Matrix4x4EffectParameter : EffectParameter<Matrix4x4>
{
    public override EffectValueTypes Type => EffectValueTypes.Matrix4x4Value;

    public Matrix4x4EffectParameter(bool isArray, Matrix4x4[] value) : base(isArray, value) { }
}
