using System;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("System.DateTime", "Microsoft.Xna.Framework.Content.DateTimeReader")]
public class DateTimeReader : SyncReader<DateTime>
{
    public DateTimeReader(XnbStreamReader rx) : base(rx) { }

    public override DateTime Read()
    {
        var packed = Rx.ReadUInt64();
        var kind = (DateTimeKind)(packed >> 62);
        var ticks = packed & ~(0b11UL << 62);

        return new DateTime((long)ticks, kind);
    }
}
