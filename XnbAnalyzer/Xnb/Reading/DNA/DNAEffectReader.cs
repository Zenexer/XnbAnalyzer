using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb.Reading.DNA
{
    [Reader("DNA.Drawing.Effects.DNAEffect", "DNA.Drawing.Effects.DNAEffect+Reader")]
    public class DNAEffectReader : Reader<DNAEffect>
    {
        public DNAEffectReader(XnbStreamReader rx) : base(rx) { }

        public override DNAEffect Read()
        {
            var effectRef = Rx.ReadExternalReference<Effect>();
            var parameters = new Dictionary<string, EffectParameter>();

            var textureCount = Rx.ReadInt32();
            for (var i = 0; i < textureCount; i++)
            {
                var paramName = Rx.ReadString();
                var textureRef = Rx.ReadExternalReference<Texture>();
                parameters[paramName] = new EffectParameter(textureRef);
            }

            var entryCount = Rx.ReadInt32();
            for (var entryIndex = 0; entryIndex < entryCount; entryIndex++)
            {
                var paramName = Rx.ReadString();
                var param = parameters.GetValueOrDefault(paramName);
                var effectValueType = (EffectValueTypes)Rx.ReadByte();
                var valueCount = Rx.ReadInt32();

				if (valueCount == 0)
				{
					var value = effectValueType switch
					{
						EffectValueTypes.intValue => new EffectParameter(Rx.ReadInt32()),
						EffectValueTypes.stringValue => new EffectParameter(Rx.ReadString()),
						EffectValueTypes.boolValue => new EffectParameter(Rx.ReadBoolean()),
						EffectValueTypes.floatValue => new EffectParameter(Rx.ReadSingle()),
						EffectValueTypes.Vector2Value => new EffectParameter(Rx.ReadVector2()),
						EffectValueTypes.Vector3Value => new EffectParameter(Rx.ReadVector3()),
						EffectValueTypes.Vector4Value => new EffectParameter(Rx.ReadVector4()),
						EffectValueTypes.MatrixValue => new EffectParameter(Rx.ReadMatrix4x4()),
						_ => throw new XnbFormatException($"Unknown {nameof(EffectValueTypes)}: {effectValueType}"),
					};
				}
				else
				{
                    switch (effectValueType)
					{
						case EffectValueTypes.intValue:
							{
								var values = new int[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadInt32();
								}
								break;
							}

						case EffectValueTypes.boolValue:
							{
								var values = new bool[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadBoolean();
								}
								break;
							}

						case EffectValueTypes.floatValue:
							{
								var values = new float[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadSingle();
								}
								break;
							}

						case EffectValueTypes.Vector2Value:
							{
								var values = new Vector2[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadVector2();
								}
								break;
							}

						case EffectValueTypes.Vector3Value:
							{
								var values = new Vector3[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadVector3();
								}
								break;
							}

						case EffectValueTypes.Vector4Value:
							{
								var values = new Vector4[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadVector4();
								}
								break;
							}

						case EffectValueTypes.MatrixValue:
							{
								var values = new Matrix4x4[valueCount];
								for (int i = 0; i < values.Length; i++)
								{
									values[i] = Rx.ReadMatrix4x4();
								}
								break;
							}

						default:
							throw new XnbFormatException($"Unknown {nameof(EffectValueTypes)}: {effectValueType}");
					}
                }
            }

			// TODO: Store effect data
			return new DNAEffect(ImmutableArray<byte>.Empty);
        }
    }
}
