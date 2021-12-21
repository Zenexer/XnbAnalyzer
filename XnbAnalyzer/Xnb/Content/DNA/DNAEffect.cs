using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content.DNA
{
    [Serializable]
    public record DNAEffect(ImmutableArray<byte> Bytecode) : Effect(Bytecode);

	public enum EffectValueTypes : byte
	{
		intValue = 0,
		stringValue = 1,
		boolValue = 2,
		floatValue = 3,
		Vector2Value = 4,
		Vector3Value = 5,
		Vector4Value = 6,
		MatrixValue = 7,
	}
}
