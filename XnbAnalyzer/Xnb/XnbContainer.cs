using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.XMemCompress;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Content.DNA;

namespace XnbAnalyzer.Xnb
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
            $"AssetType = {Asset?.GetType().FullName ?? "(null)"}",
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

        public static async Task<XnbContainer> ReadFromFileAsync(string contentRoot, string assetPath, CancellationToken cancellationToken)
        {
            contentRoot = Path.GetFullPath(contentRoot);
            var assetName = Path.ChangeExtension(assetPath, null);

            using var fileReader = File.OpenRead(Path.Combine(contentRoot, assetPath));

            return await ReadAsync(
                contentRoot,
                assetName,
                fileReader,
                // TODO: Expose an option for saving an intermediate file
#if DEBUG && false
                Path.Combine(contentRoot, assetPath + ".bin"),
#else
                null,
#endif
                cancellationToken
            );
        }

        // TODO Make the rest of this async
        public static async Task<XnbContainer> ReadAsync(string contentRoot, string assetName, Stream input, string? intermediateFile, CancellationToken cancellationToken)
        {
            var rx = new XnbStreamReader(contentRoot, assetName, input, true);
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

                    if (intermediateFile is not null)
                    {
                        using (var intermediate = File.OpenWrite(intermediateFile))
                        {
                            await buffer.CopyToAsync(intermediate, cancellationToken);
                            intermediate.Flush();
                        }

                        buffer.Position = 0;
                    }

                    rx = new XnbStreamReader(contentRoot, assetName, buffer, true);
                }

                var typeReaders = rx.ReadTypeReaderCollection();
                var sharedResourceCount = rx.Read7BitEncodedInt();
                var asset = await rx.ReadObjectAsync(cancellationToken);

                var sharedResources = new object?[sharedResourceCount];
                for (var i = 0; i < sharedResourceCount; i++)
                {
                    sharedResources[i] = await rx.ReadObjectAsync(cancellationToken);
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

        public async Task ExportAsync(string path, CancellationToken cancellationToken)
        {
            var parentDir = Path.GetDirectoryName(path);
            if (parentDir is not null && !Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            switch (Asset)
            {
                case ImmutableArray<Texture> textures:
                    Directory.CreateDirectory(path);

                    for (var i = 0; i < textures.Length; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await textures[i].ExportAsync(Path.Combine(path, i.ToString()), cancellationToken);
                    }
                    break;

                case IExportable exportable:
                    await exportable.ExportAsync(path, cancellationToken);
                    break;

                case Model _:
                case AnimationClip _:
                    Directory.CreateDirectory(path);

                    var options = new JsonSerializerOptions { WriteIndented = true };

                    using (var tx = File.Create(Path.Combine(path, Asset.GetType().Name + ".json")))
                    {
                        await JsonSerializer.SerializeAsync(tx, Asset, Asset.GetType(), options, cancellationToken);
                    }

                    if (!SharedResources.IsEmpty)
                    {
                        using var tx = File.Create(Path.Combine(path, "SharedResources.json"));
                        await JsonSerializer.SerializeAsync(tx, SharedResources, options, cancellationToken);
                    }
                    break;

                default:
                    throw new NotImplementedException($"No save implementation for {Asset?.GetType().GetFullName() ?? "(null)"}");
            }
        }
    }
}
