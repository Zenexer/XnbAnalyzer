using System;

namespace XnbAnalyzer.Xnb.Reading;

[Reader("System.TimeSpan", "Microsoft.Xna.Framework.Content.TimeSpanReader")]
public class TimeSpanReader : SyncReader<TimeSpan>
{
    public TimeSpanReader(XnbStreamReader rx) : base(rx) { }

    public override TimeSpan Read() => TimeSpan.FromTicks(Rx.ReadInt64());
}
