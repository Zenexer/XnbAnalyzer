using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("System.TimeSpan", "Microsoft.Xna.Framework.Content.TimeSpanReader")]
    public class TimeSpanReader : Reader<TimeSpan>
    {
        public TimeSpanReader(XnbStreamReader rx) : base(rx) { }

        public override TimeSpan Read() => TimeSpan.FromTicks(Rx.ReadInt64());
    }

    [Reader("System.DateTime", "Microsoft.Xna.Framework.Content.DateTimeReader")]
    public class DateTimeReader : Reader<DateTime>
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

    // XNB doesn't consider this a primitive
    [Reader("System.Decimal", "Microsoft.Xna.Framework.Content.DecimalReader")]
    public class DecimalReader : Reader<decimal>
    {
        public DecimalReader(XnbStreamReader rx) : base(rx) { }

        public override decimal Read() => Rx.ReadDecimal();
    }
}
