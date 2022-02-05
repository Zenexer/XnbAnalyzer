using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA;

[Reader("DNA.Drawing.Effects.DNAEffect", "DNA.Drawing.Effects.DNAEffect+Reader")]
public class DNAEffectReader : SyncReader<DNAEffect>
{
    public DNAEffectReader(XnbStreamReader rx) : base(rx) { }

    private EffectParameter<T> ReadParameterValue<T>(int valueCount, Func<T> read, Func<bool, T[], EffectParameter<T>> wrap)
        where T : struct
    {
        var readCount = valueCount == 0 ? 1 : valueCount;
        var values = new T[readCount];

        for (var i = 0; i < readCount; i++)
        {
            values[i] = read();
        }

        return wrap(valueCount != 0, values);
    }

    public override DNAEffect Read()
    {
        var effectRef = Rx.ReadExternalReference<Effect>();
        var textureRefs = new Dictionary<string, ExternalReference<Texture>>();
        var parameters = new Dictionary<string, EffectParameter>();

        var textureCount = Rx.ReadInt32();
        for (var i = 0; i < textureCount; i++)
        {
            var paramName = Rx.ReadString();
            var textureRef = Rx.ReadExternalReference<Texture>();
            textureRefs[paramName] = textureRef;
        }

        var entryCount = Rx.ReadInt32();
        for (var entryIndex = 0; entryIndex < entryCount; entryIndex++)
        {
            var paramName = Rx.ReadString();
            var effectValueType = (EffectValueTypes)Rx.ReadByte();
            var valueCount = Rx.ReadInt32();

            parameters[paramName] = effectValueType switch
            {
                EffectValueTypes.Int32Value     => ReadParameterValue<int>(valueCount,       () => Rx.ReadInt32(),     (isArray, values) => new Int32EffectParameter(isArray, values)),
                EffectValueTypes.BooleanValue   => ReadParameterValue<bool>(valueCount,      () => Rx.ReadBoolean(),   (isArray, values) => new BooleanEffectParameter(isArray, values)),
                EffectValueTypes.SingleValue    => ReadParameterValue<float>(valueCount,     () => Rx.ReadSingle(),    (isArray, values) => new SingleEffectParameter(isArray, values)),
                EffectValueTypes.Vector2Value   => ReadParameterValue<Vector2>(valueCount,   () => Rx.ReadVector2(),   (isArray, values) => new Vector2EffectParameter(isArray, values)),
                EffectValueTypes.Vector3Value   => ReadParameterValue<Vector3>(valueCount,   () => Rx.ReadVector3(),   (isArray, values) => new Vector3EffectParameter(isArray, values)),
                EffectValueTypes.Vector4Value   => ReadParameterValue<Vector4>(valueCount,   () => Rx.ReadVector4(),   (isArray, values) => new Vector4EffectParameter(isArray, values)),
                EffectValueTypes.Matrix4x4Value => ReadParameterValue<Matrix4x4>(valueCount, () => Rx.ReadMatrix4x4(), (isArray, values) => new Matrix4x4EffectParameter(isArray, values)),
                _ => throw new XnbFormatException($"Unknown {nameof(EffectValueTypes)}: {effectValueType}"),
            };
        }

        return new(
            effectRef,
            textureRefs.ToImmutableDictionary(),
            parameters.ToImmutableDictionary()
        );
    }
}
