﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb.Content
{
    public abstract class Texture
    {
        public abstract Task SaveToFolderAsync(string dir);
    }
}
