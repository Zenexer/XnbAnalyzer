using System;
using System.IO;
using System.Threading.Tasks;
using XnbAnalzyer.Xnb;

namespace XnbAnalzyer
{
    public class Program
    {
        public static async Task Main(string[] args) => await UnpackFolderAsync("packed", "unpacked");

        private static async Task UnpackFolderAsync(string packedDir, string unpackedDir)
        {
            Directory.CreateDirectory(packedDir);

            foreach (var entry in Directory.GetFileSystemEntries(packedDir))
            {
                var noExt = Path.GetFileNameWithoutExtension(entry);
                var name = Path.GetFileName(entry);
                var ext = Path.GetExtension(entry);

                if (Directory.Exists(entry))
                {
                    await UnpackFolderAsync(entry, Path.Combine(unpackedDir, name));
                }
                else if (".xnb".Equals(ext, StringComparison.InvariantCultureIgnoreCase))
                {
                    var container = XnbContainer.ReadFromFile(entry);
                    Console.WriteLine(container);

                    await container.SaveToFolderAsync(Path.Combine(unpackedDir, noExt));
                }
            }
        }
    }
}
