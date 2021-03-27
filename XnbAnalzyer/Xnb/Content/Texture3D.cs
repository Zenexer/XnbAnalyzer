using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb.Content
{
    public class Texture3D : Texture
    {
        public SurfaceFormat SurfaceFormat { get; init; }
        public uint Width { get; init; }
        public uint Height { get; init; }
        public uint Depth { get; init; }
        public ImmutableArray<ImmutableArray<byte>> MipImages { get; init; }

        public override Task SaveToFolderAsync(string dir)
        {
            throw new NotImplementedException();
        }
    }
}
