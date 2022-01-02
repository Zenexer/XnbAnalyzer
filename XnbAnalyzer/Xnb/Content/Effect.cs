using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public record Effect(ReadOnlyMemory<byte> Bytecode) : IExportable
{
	public async Task ExportAsync(string path, CancellationToken cancellationToken)
	{
        var file = $"{path}.fx";

        using var tx = File.Create(file);
        await tx.WriteAsync(Bytecode, cancellationToken);
	}
}
