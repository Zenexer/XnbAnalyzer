using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Content
{
    [Serializable]
    public record Model(
        ImmutableArray<Bone> Bones,
        ImmutableArray<Mesh> Meshes,
        uint RootBoneId,
        object? Tag
    );

    [Serializable]
    public record Bone(
        string? Name,
        Matrix4x4 Transform,
        uint Parent,
        ImmutableArray<uint> Children
    );

    [Serializable]
    public record Mesh(
        string? Name,
        uint ParentBone,
        BoundingSphere Bounds,
        object? Tag,
        ImmutableArray<MeshPart> Parts
    );

    [Serializable]
    public record MeshPart(
        uint VertexOffset,
        uint VertexCount,
        uint StartIndex,
        uint PrimitiveCount,
        object? Tag,
        SharedResourceRef<VertexBuffer> VertexBuffer,
        SharedResourceRef<IndexBuffer> IndexBuffer,
        SharedResourceRef<Effect> Effect
    );
}
