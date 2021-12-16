using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Reading;

namespace XnbAnalyzer.Xnb
{
    public class XnbStreamReader : BinaryReader
    {
        private TypeReaderCollection? _typeReaders;
        private TypeReaderCollection TypeDefinitions => _typeReaders ?? throw new Exception("Neglected to read type reader definitions before reading objects");


        public XnbStreamReader(Stream input, bool leaveOpen) : base(input, Encoding.UTF8, leaveOpen) { }

        protected T AssertEnumExists<T>(T value, bool allowDefault) where T : struct, Enum
        {
            if (!Enum.IsDefined(value))
            {
                throw new XnbFormatException($"{typeof(T).FullName} doesn't contain a value corresponding to {value}");
            }

            if (!allowDefault && value.Equals(new T()))
            {
                throw new XnbFormatException($"{typeof(T).FullName} must not be {value}");
            }

            return value;
        }

        protected T AssertFlagsExist<T>(T value) where T : struct, Enum
        {
            var intVal = Convert.ToInt32(value);
            var unknownFlags = intVal & ~Enum.GetValues<T>()
                .Select(x => Convert.ToInt32(x))
                .Aggregate((a, b) => a | b);

            if (unknownFlags != 0)
            {
                throw new XnbFormatException($"Unrecognized flag(s) in {typeof(T).FullName}: {unknownFlags:X}");
            }

            return value;
        }

        public Vector2 ReadVector2() => new Vector2(ReadSingle(), ReadSingle());
        public Vector3 ReadVector3() => new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        public Vector4 ReadVector4() => new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        public Quaternion ReadQuaternion() => new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        public Matrix4x4 ReadMatrix4x4() => new Matrix4x4(
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle(),
            ReadSingle()
        );

        public TargetPlatform ReadTargetPlatform() => AssertEnumExists((TargetPlatform)ReadByte(), false);

        public FormatVersion ReadFormatVersion() => AssertEnumExists((FormatVersion)ReadByte(), false);

        public XnbFlags ReadXnbFlags() => AssertFlagsExist((XnbFlags)ReadByte());

        public SurfaceFormat ReadSurfaceFormat() => AssertEnumExists((SurfaceFormat)ReadInt32(), true);

        public TypeDefinition ReadTypeReaderDefinition() => new TypeDefinition(ReadString(), ReadInt32());

        public TypeReaderCollection ReadTypeReaderCollection()
        {
            var definitions = new TypeDefinition[Read7BitEncodedInt()];

            for (var i = 0; i < definitions.Length; i++)
            {
                definitions[i] = ReadTypeReaderDefinition();
            }

            return _typeReaders = new TypeReaderCollection(definitions);
        }

        public SharedResourceRef<T> ReadSharedResourceRef<T>() => new SharedResourceRef<T>(Read7BitEncodedInt());

        public ExternalReference<T> ReadExternalReference<T>() => new ExternalReference<T>(ReadString());

        public TypeDefinition? ReadObjectHeader()
        {
            var typeIndex = Read7BitEncodedInt();

            if (typeIndex == 0)
            {
                return null;
            }

            if (typeIndex < 0)
            {
                throw new XnbFormatException("Negative object type index");
            }

            typeIndex--;  // 0 is reserved for null; if it's not null, subtract 1 to get the real type ID

            if (typeIndex >= TypeDefinitions.Count)
            {
                throw new XnbFormatException($"Type reader definition {typeIndex} doesn't exist");
            }

            return TypeDefinitions[typeIndex];
        }

        public object? ReadObject()
        {
            if (ReadObjectHeader() is not { } typeDefinition)
            {
                return null;
            }

            var reader = ContentTypeMapper.Instance.GetReader(this, typeDefinition);
            return reader.ReadObject();
        }

        public T? ReadObject<T>()
        {
            if (typeof(T).IsValueType && typeof(T).Name != "ImmutableArray`1")
            {
                return ReadDirect<T>();
            }

            if (ReadObjectHeader() is not { } typeDefinition)
            {
                return default;
            }

            var type = ContentTypeMapper.Instance.GetContentType(typeDefinition);

            if (!type.IsAssignableTo(typeof(T)))
            {
                throw new XnbFormatException($"Expected to read {typeof(T).FullName}, but got {type.FullName}");
            }

            return (T?)ContentTypeMapper.Instance.GetReader(this, typeDefinition).ReadObject();
        }

        public T ReadDirect<T>()
        {
            return ContentTypeMapper.Instance.GetReader<T>(this).Read();
        }
    }
}
