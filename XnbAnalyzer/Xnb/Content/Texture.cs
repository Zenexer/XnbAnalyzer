﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    public abstract class Texture : IExportable
    {
        public async Task ExportAsync(string path, CancellationToken cancellationToken)
            => await SaveToFolderAsync(path, cancellationToken);

        public abstract Task SaveToFolderAsync(string dir, CancellationToken cancellationToken);
    }
}
