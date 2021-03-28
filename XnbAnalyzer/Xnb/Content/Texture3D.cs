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
        public SurfaceFormat SurfaceFormat { get; init; }
        public uint Width { get; init; }
        public uint Height { get; init; }
        public uint Depth { get; init; }
        public ImmutableArray<ImmutableArray<byte>> MipImages { get; init; }

        public override Task SaveToFolderAsync(string dir, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
