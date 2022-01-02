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

    private object ReadParameterValue<T>(int valueCount, Func<T> read)
        where T : struct
    {
        var readCount = valueCount == 0 ? 1 : valueCount;
        var values = new T[readCount];

        for (var i = 0; i < readCount; i++)
        {
            values[i] = read();
        }

        return valueCount == 0 ? values[0] : values.ToImmutableArray();
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
            var readCount = valueCount == 0 ? 1 : valueCount;
            
            var value = effectValueType switch
            {
                EffectValueTypes.intValue => ReadParameterValue<int>(valueCount, () => Rx.ReadInt32()),
                EffectValueTypes.boolValue => ReadParameterValue<bool>(valueCount, () => Rx.ReadBoolean()),
                EffectValueTypes.floatValue => ReadParameterValue<float>(valueCount, () => Rx.ReadSingle()),
                EffectValueTypes.Vector2Value => ReadParameterValue<Vector2>(valueCount, () => Rx.ReadVector2()),
                EffectValueTypes.Vector3Value => ReadParameterValue<Vector3>(valueCount, () => Rx.ReadVector3()),
                EffectValueTypes.Vector4Value => ReadParameterValue<Vector4>(valueCount, () => Rx.ReadVector4()),
                EffectValueTypes.MatrixValue => ReadParameterValue<Matrix4x4>(valueCount, () => Rx.ReadMatrix4x4()),
                _ => throw new XnbFormatException($"Unknown {nameof(EffectValueTypes)}: {effectValueType}"),
            };

            parameters[paramName] = new EffectParameter(value);
        }

        return new(
            effectRef,
            textureRefs.ToImmutableDictionary(),
            parameters.ToImmutableDictionary()
        );
    }
}
