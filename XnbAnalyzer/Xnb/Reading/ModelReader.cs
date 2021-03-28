using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Graphics.Model", "Microsoft.Xna.Framework.Content.ModelReader")]
    public class ModelReader : Reader<Model>
    {
        public ModelReader(XnbStreamReader rx) : base(rx)
        {
        }

        public override Model Read()
        {
            var boneCount = Rx.ReadUInt32();
            var transforms = new (string? Name, Matrix4x4 Transform)[boneCount];
            var bones = new Bone[boneCount];

            Func<uint> readBoneRef = boneCount < 255 ? () => Rx.ReadByte() : () => Rx.ReadUInt32();

            for (var boneId = 0; boneId < boneCount; boneId++)
            {
                transforms[boneId] = (Rx.ReadObject<string>(), Rx.ReadMatrix4x4());
            }

            for (var boneId = 0; boneId < boneCount; boneId++)
            {
                var parent = readBoneRef();
                var childBoneCount = Rx.ReadUInt32();
                var childBones = new uint[childBoneCount];
                
                for (var i = 0; i < childBoneCount; i++)
                {
                    childBones[i] = readBoneRef();
                }

                bones[boneId] = new Bone(
                    transforms[boneId].Name,
                    transforms[boneId].Transform,
                    parent,
                    childBones.ToImmutableArray()
                );
            }

            var meshCount = Rx.ReadUInt32();
            var meshes = new Mesh[meshCount];
            for (var meshId = 0; meshId < meshCount; meshId++)
            {
                var name = Rx.ReadObject<string>();
                var parent = readBoneRef();
                var boundingSphere = Rx.ReadDirect<BoundingSphere>();
                var tag = Rx.ReadObject();
                var partCount = Rx.ReadUInt32();
                var parts = new MeshPart[partCount];

                for (var i = 0; i < partCount; i++)
                {
                    parts[i] = new MeshPart(
                        Rx.ReadUInt32(),
                        Rx.ReadUInt32(),
                        Rx.ReadUInt32(),
                        Rx.ReadUInt32(),
                        Rx.ReadObject(),
                        Rx.ReadSharedResourceRef<VertexBuffer>(),
                        Rx.ReadSharedResourceRef<IndexBuffer>(),
                        Rx.ReadSharedResourceRef<Effect>()
                    );
                }

                meshes[meshId] = new Mesh(name, parent, boundingSphere, tag, parts.ToImmutableArray());
            }

            var rootBone = readBoneRef();
            var modelTag = Rx.ReadObject();

            return new Model(bones.ToImmutableArray(), meshes.ToImmutableArray(), rootBone, modelTag);
        }
    }
}
