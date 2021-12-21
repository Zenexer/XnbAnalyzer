using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("System.Byte", "Microsoft.Xna.Framework.Content.ByteReader", IsPrimitive = true)] public class ByteReader : SyncReader<byte> { public ByteReader(XnbStreamReader rx) : base(rx) { } public override byte Read() => Rx.ReadByte(); }
    [Reader("System.SByte", "Microsoft.Xna.Framework.Content.SByteReader", IsPrimitive = true)] public class SByteReader : SyncReader<sbyte> { public SByteReader(XnbStreamReader rx) : base(rx) { } public override sbyte Read() => Rx.ReadSByte(); }
    [Reader("System.Int16", "Microsoft.Xna.Framework.Content.Int16Reader", IsPrimitive = true)] public class Int16Reader : SyncReader<short> { public Int16Reader(XnbStreamReader rx) : base(rx) { } public override short Read() => Rx.ReadInt16(); }
    [Reader("System.UInt16", "Microsoft.Xna.Framework.Content.UInt16Reader", IsPrimitive = true)] public class UInt16Reader : SyncReader<ushort> { public UInt16Reader(XnbStreamReader rx) : base(rx) { } public override ushort Read() => Rx.ReadUInt16(); }
    [Reader("System.Int32", "Microsoft.Xna.Framework.Content.Int32Reader", IsPrimitive = true)] public class Int32Reader : SyncReader<int> { public Int32Reader(XnbStreamReader rx) : base(rx) { } public override int Read() => Rx.ReadInt32(); }
    [Reader("System.UInt32", "Microsoft.Xna.Framework.Content.UInt32Reader", IsPrimitive = true)] public class UInt32Reader : SyncReader<uint> { public UInt32Reader(XnbStreamReader rx) : base(rx) { } public override uint Read() => Rx.ReadUInt32(); }
    [Reader("System.Int64", "Microsoft.Xna.Framework.Content.Int64Reader", IsPrimitive = true)] public class Int64Reader : SyncReader<long> { public Int64Reader(XnbStreamReader rx) : base(rx) { } public override long Read() => Rx.ReadInt64(); }
    [Reader("System.UInt64", "Microsoft.Xna.Framework.Content.UInt64Reader", IsPrimitive = true)] public class UInt64Reader : SyncReader<ulong> { public UInt64Reader(XnbStreamReader rx) : base(rx) { } public override ulong Read() => Rx.ReadUInt64(); }
    [Reader("System.Single", "Microsoft.Xna.Framework.Content.SingleReader", IsPrimitive = true)] public class SingleReader : SyncReader<float> { public SingleReader(XnbStreamReader rx) : base(rx) { } public override float Read() => Rx.ReadSingle(); }
    [Reader("System.Double", "Microsoft.Xna.Framework.Content.DoubleReader", IsPrimitive = true)] public class DoubleReader : SyncReader<double> { public DoubleReader(XnbStreamReader rx) : base(rx) { } public override double Read() => Rx.ReadDouble(); }
    [Reader("System.Boolean", "Microsoft.Xna.Framework.Content.BooleanReader", IsPrimitive = true)] public class BooleanReader : SyncReader<bool> { public BooleanReader(XnbStreamReader rx) : base(rx) { } public override bool Read() => Rx.ReadBoolean(); }
    [Reader("System.Char", "Microsoft.Xna.Framework.Content.CharReader", IsPrimitive = true)] public class CharReader : SyncReader<char> { public CharReader(XnbStreamReader rx) : base(rx) { } public override char Read() => Rx.ReadChar(); }
    [Reader("System.String", "Microsoft.Xna.Framework.Content.StringReader", IsPrimitive = true)] public class StringReader : SyncReader<string> { public StringReader(XnbStreamReader rx) : base(rx) { } public override string Read() => Rx.ReadString(); }
}
