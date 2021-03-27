using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalzyer.XMemCompress;
using XnbAnalzyer.Xnb.Content;

namespace XnbAnalzyer.Xnb
{
    public class XnbContainer
    {
        private static readonly (byte, byte, byte) Magic = ((byte)'X', (byte)'N', (byte)'B');

        public TargetPlatform TargetPlatform { get; set; }
        public FormatVersion FormatVersion { get; set; }
        public XnbFlags Flags { get; set; }
        public object? Asset { get; set; }
        public ImmutableArray<object?> SharedResources { get; set; }

        protected XnbContainer() { }

        public override string? ToString() => string.Join("\n", new[]
        {
            $"TargetPlatform = {TargetPlatform}",
            $"FormatVersion = {FormatVersion}",
            $"Flags = {Flags}",
            $"SharedResourceCount = {SharedResources.Length}",
            $"Asset =",
            Asset switch
            {
                null => "(null)",
                IEnumerable x => "  - " + string.Join("\n  - ", x.Cast<object>().Select(y => y?.ToString() ?? "(null)")),
                var x => x.ToString(),
            },
        });

        public static XnbContainer ReadFromFile(string path)
        {
            using var fileReader = File.OpenRead(path);

            return Read(fileReader);
        }

        public static XnbContainer Read(Stream input)
        {
            var rx = new XnbStreamReader(input, true);
            MemoryStream? buffer = null;
            try
            {

                var magic = (rx.ReadByte(), rx.ReadByte(), rx.ReadByte());

                if (magic != Magic)
                {
                    throw new XnbFormatException("Bad magic value in header");
                }

                var targetPlatform = rx.ReadTargetPlatform();
                var formatVersion = rx.ReadFormatVersion();
                var flags = rx.ReadXnbFlags();
                var compressedFileSize = rx.ReadUInt32();
                var compressedDataSize = compressedFileSize - 14;  // Compressed data stream begins 14 bytes in
                var decompressedDataSize = flags.HasFlag(XnbFlags.CompressedLZX) ? rx.ReadUInt32() : 0;

                if (decompressedDataSize >= (1 << 30))  // 1 GiB
                {
                    throw new XnbFormatException("Decompressed data size is unreasonably large");
                }

                if (compressedFileSize != input.Length)
                {
                    throw new XnbFormatException("Compressed file size doesn't match actual file size");
                }

                if (flags.HasFlag(XnbFlags.CompressedLZX))
                {
                    input.Position = 14;

                    var lzx = new LzxDecoder(16);
                    buffer = new MemoryStream((int)decompressedDataSize);

                    while (input.Position < input.Length)
                    {
                        var blockSize = (input.ReadByte() << 8) | input.ReadByte();
                        var frameSize = 0x8000;

                        if ((blockSize >> 8) == 0xff)
                        {
                            if (input.Position + 3 > input.Length)
                            {
                                throw new XnbFormatException("Bad LZX block header");
                            }

                            frameSize = ((blockSize & 0xff) << 8) | input.ReadByte();
                            blockSize = (input.ReadByte() << 8) | input.ReadByte();
                        }

                        if (blockSize == 0 || frameSize == 0)
                        {
                            break;
                        }

                        var targetPosition = input.Position + blockSize;
                        var result = lzx.Decompress(input, blockSize, buffer, frameSize);

                        if (result != 0)
                        {
                            throw new XnbFormatException("LZX decoder indicated an error");
                        }

                        if (input.Position != targetPosition)
                        {
                            input.Position = targetPosition;
                        }
                    }

                    buffer.Position = 0;

                    rx.Dispose();
                    rx = new XnbStreamReader(buffer, true);
                }

                var typeReaders = rx.ReadTypeReaderCollection();
                var sharedResourceCount = rx.Read7BitEncodedInt();
                var asset = rx.ReadObject();

                var sharedResources = new object?[sharedResourceCount];
                for (var i = 0; i < sharedResourceCount; i++)
                {
                    sharedResources[i] = rx.ReadObject();
                }

                return new XnbContainer
                {
                    TargetPlatform = targetPlatform,
                    FormatVersion = formatVersion,
                    Flags = flags,
                    Asset = asset,
                    SharedResources = sharedResources.ToImmutableArray(),
                };
            }
            finally
            {
                rx.Dispose();

                if (buffer != null)
                {
                    buffer.Dispose();
                }
            }
        }

        public async Task SaveToFolderAsync(string dir)
        {
            Directory.CreateDirectory(dir);

            switch (Asset)
            {
                case ImmutableArray<Texture> textures:
                    for (var i = 0; i < textures.Length; i++)
                    {
                        await textures[i].SaveToFolderAsync(Path.Combine(dir, i.ToString()));
                    }
                    break;

                case Texture2D texture:
                    await texture.SaveToFolderAsync(dir);
                    break;

                default:
                    throw new NotImplementedException($"No save implementation for {Asset?.GetType().GetFullName() ?? "(null)"}");
            }
        }
    }
}
