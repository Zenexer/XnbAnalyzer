using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading;

// XNB doesn't consider this a primitive
[Reader("System.Decimal", "Microsoft.Xna.Framework.Content.DecimalReader")]
public class DecimalReader : SyncReader<decimal>
{
    public DecimalReader(XnbStreamReader rx) : base(rx) { }

    public override decimal Read() => Rx.ReadDecimal();
}
