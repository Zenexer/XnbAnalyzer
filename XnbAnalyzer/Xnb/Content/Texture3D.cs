using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public class Texture3D : Texture
    {
        public uint Depth { get; init; }

        protected override Task SaveToFolderAsync(string dir, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
