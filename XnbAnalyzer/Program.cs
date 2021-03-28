using McMaster.Extensions.CommandLineUtils;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb;

namespace XnbAnalyzer
{
    [Command(Name = "xnb", Description = "XNB packer and unpacker"), Subcommand(typeof(Pack), typeof(Unpack))]
    public class Program
    {
        public static async Task Main(string[] args)
            => await CommandLineApplication.ExecuteAsync<Program>(args);

        [Command("pack", Description = "Pack data and files into an XNB container")]
        public class Pack
        {
            public Task OnExecuteAsync(CancellationToken cancellationToken)
                => throw new NotImplementedException();
        }

        [Command("unpack", Description = "Extract data and files from an XNB container"), Subcommand(typeof(Dir))]
        public class Unpack
        {
            [Command("dir", Description = "Recursively unpack an entire directory on XNB files")]
            public class Dir
            {
                [Required]
                [Argument(0, Description = "The folder containing packed XNB files")]
                public string? InputDir { get; set; }

                [Required]
                [Argument(1, Description = "The folder to which the XNB files will be unpacked, preserving directory structure")]
                public string? OutputDir { get; set; }

                [Option("-r|--recursive", Description = "Recurse into directories while finding XNB files")]
                public bool IsRecursive { get; set; }

                public async Task OnExecuteAsync(CancellationToken cancellationToken)
                    => await UnpackFolderAsync(
                        InputDir ?? throw new NullReferenceException(nameof(InputDir)),
                        OutputDir ?? throw new NullReferenceException(nameof(OutputDir)),
                        cancellationToken
                    );

                public async Task UnpackFolderAsync(string packedDir, string unpackedDir, CancellationToken cancellationToken)
                {
                    Directory.CreateDirectory(packedDir);

                    foreach (var entry in Directory.GetFileSystemEntries(packedDir))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var noExt = Path.GetFileNameWithoutExtension(entry);
                        var name = Path.GetFileName(entry);
                        var ext = Path.GetExtension(entry);

                        if (Directory.Exists(entry))
                        {
                            if (IsRecursive)
                            {
                                await UnpackFolderAsync(entry, Path.Combine(unpackedDir, name), cancellationToken);
                            }
                        }
                        else if (".xnb".Equals(ext, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var container = XnbContainer.ReadFromFile(entry);
                            Console.WriteLine(container);

                            await container.SaveToFolderAsync(Path.Combine(unpackedDir, noExt), cancellationToken);
                        }
                    }
                }
            }
        }
    }
}
