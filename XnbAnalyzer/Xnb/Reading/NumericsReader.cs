using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalyzer.Xnb.Reading
{
    [Reader("Microsoft.Xna.Framework.Vector2", "Microsoft.Xna.Framework.Content.Vector2Reader")]
    public class Vector2Reader : Reader<Vector2>
    {
        public Vector2Reader(XnbStreamReader rx) : base(rx) { }

        public override Vector2 Read() => Rx.ReadVector2();
    }

    [Reader("Microsoft.Xna.Framework.Vector3", "Microsoft.Xna.Framework.Content.Vector3Reader")]
    public class Vector3Reader : Reader<Vector3>
    {
        public Vector3Reader(XnbStreamReader rx) : base(rx) { }

        public override Vector3 Read() => Rx.ReadVector3();
    }

    [Reader("Microsoft.Xna.Framework.Vector4", "Microsoft.Xna.Framework.Content.Vector4Reader")]
    public class Vector4Reader : Reader<Vector4>
    {
        public Vector4Reader(XnbStreamReader rx) : base(rx) { }

        public override Vector4 Read() => Rx.ReadVector4();
    }

    [Reader("Microsoft.Xna.Framework.Quaternion", "Microsoft.Xna.Framework.Content.QuaternionReader")]
    public class QuaternionReader : Reader<Quaternion>
    {
        public QuaternionReader(XnbStreamReader rx) : base(rx) { }

        public override Quaternion Read() => Rx.ReadQuaternion();
    }

    [Reader("Microsoft.Xna.Framework.Matrix", "Microsoft.Xna.Framework.Content.MatrixReader")]
    public class MatrixReader : Reader<Matrix4x4>
    {
        public MatrixReader(XnbStreamReader rx)
            : base(rx) { }

        public override Matrix4x4 Read() => Rx.ReadMatrix4x4();
    }
}
