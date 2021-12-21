using System;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
public record Sprite(Texture2D Texture, Rectangle SourceRectangle);
