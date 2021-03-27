using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.XMemCompress
{
    public class XMemCompressDecoderStream : Stream
    {
        public Stream BaseStream { get; init; }
        public int DecompressedSize { get; init; }
        public int CompressedSize { get; init; }

        public XMemCompressDecoderStream(Stream baseStream, int decompressedSize, int compressedSize)
        {
            BaseStream = baseStream;
            DecompressedSize = decompressedSize;
            CompressedSize = compressedSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseStream.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await BaseStream.DisposeAsync();
            await base.DisposeAsync();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => DecompressedSize;

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
