using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Content;
using XnbAnalyzer.Xnb.Reading;

namespace XnbAnalyzer.Xnb
{
    public class XnbStreamReader : BinaryReader
    {
        private TypeReaderCollection? _typeReaders;
        private TypeReaderCollection TypeDefinitions => _typeReaders ?? throw new Exception("Neglected to read type reader definitions before reading objects");
        public string ContentRoot { get; }
        public string AssetName { get; }
        public string ReferenceRoot => Path.GetDirectoryName(Path.Combine(ContentRoot, AssetName)) ?? throw new InvalidOperationException();

        public XnbStreamReader(string contentRoot, string assetName, Stream input, bool leaveOpen)
            : base(input, Encoding.UTF8, leaveOpen)
        {
            ContentRoot = contentRoot;
            AssetName = assetName;
        }

        protected static T AssertEnumExists<T>(T value, bool allowDefault) where T : struct, Enum
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

        protected static T AssertFlagsExist<T>(T value, bool allowDefault = true) where T : struct, Enum
        {
            var intVal = Convert.ToInt32(value);
            if (!allowDefault && intVal == 0)
            {
                throw new XnbFormatException($"{typeof(T).FullName} must not be {value}");
            }

            var unknownFlags = intVal & ~Enum.GetValues<T>()
                .Select(x => Convert.ToInt32(x))
                .Aggregate((a, b) => a | b);

            if (unknownFlags != 0)
            {
                throw new XnbFormatException($"Unrecognized flag(s) in {typeof(T).FullName}: {unknownFlags:X}");
            }

            return value;
        }

        public async ValueTask ReadBytesAsync(Memory<byte> bytes, CancellationToken cancellationToken)
        {
            var length = 0;
            int read;

            do
            {
                read = await BaseStream.ReadAsync(bytes[length..], cancellationToken);
                length += read;
            }
            while (read > 0);
        }

        public nint ReadNativeInt() => ReadUnmanaged<nint>();
        public nuint ReadNativeUInt() => ReadUnmanaged<nuint>();
        public Vector2 ReadVector2() => new(ReadSingle(), ReadSingle());
        public Vector3 ReadVector3() => new(ReadSingle(), ReadSingle(), ReadSingle());
        public Vector4 ReadVector4() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        public Quaternion ReadQuaternion() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        public Matrix4x4 ReadMatrix4x4() => new(
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

        private T ReadUnmanaged<T>() where T : unmanaged
        {
            Span<T> value = stackalloc T[1];
            Read(MemoryMarshal.AsBytes(value));
            return value[0];
        }

        public T ReadEnum<T>(bool isFlagType, bool allowDefault)
            where T : unmanaged, Enum
        {
            var value = ReadUnmanaged<T>();

            return isFlagType ? AssertFlagsExist(value, allowDefault) : AssertEnumExists(value, allowDefault);
        }

        public TargetPlatform ReadTargetPlatform() => ReadEnum<TargetPlatform>(false, false);

        public FormatVersion ReadFormatVersion() => ReadEnum<FormatVersion>(false, false);

        public XnbFlags ReadXnbFlags() => ReadEnum<XnbFlags>(true, true);

        public SurfaceFormat ReadSurfaceFormat() => AssertEnumExists((SurfaceFormat)ReadInt32(), true);

        public TypeDefinition ReadTypeReaderDefinition() => new(ReadString(), ReadInt32());

        public TypeReaderCollection ReadTypeReaderCollection()
        {
            var definitions = new TypeDefinition[Read7BitEncodedInt()];

            for (var i = 0; i < definitions.Length; i++)
            {
                definitions[i] = ReadTypeReaderDefinition();
            }

            return _typeReaders = new TypeReaderCollection(definitions);
        }

        public SharedResourceRef<T> ReadSharedResourceRef<T>() => new(Read7BitEncodedInt());

        public ExternalReference<T> ReadExternalReference<T>() => new(ReadString());

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

        public async ValueTask<object?> ReadObjectAsync(CancellationToken cancellationToken)
        {
            if (ReadObjectHeader() is not { } typeDefinition)
            {
                return null;
            }

            var reader = ContentTypeMapper.Instance.GetReader(this, typeDefinition);
            return await reader.ReadObjectAsync(cancellationToken);
        }

        /// <summary>
        /// Convenience method that will throw an exception if there's an encoding issue that causes a non-nullable boject to be read as null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        /// <exception cref="XnbFormatException"></exception>
        public async ValueTask<T> ReadObjectNonNullAsync<T>(string propertyName, CancellationToken cancellationToken)
            => await ReadObjectAsync<T>(cancellationToken) ?? throw new XnbFormatException($"Null reflective field: {propertyName}");

        public async ValueTask<T?> ReadObjectAsync<T>(CancellationToken cancellationToken)
        {
            if (typeof(T).IsValueType && typeof(T).Name != "ImmutableArray`1")
            {
                return await ReadDirectAsync<T>(cancellationToken);
            }

            if (ReadObjectHeader() is not { } typeDefinition)
            {
                return default;
            }

            var type = ContentTypeMapper.Instance.GetContentType(typeDefinition);

            if (type == typeof(ExternalReference<>))
            {
                var externalReference = await ContentTypeMapper.Instance.GetReader<ExternalReference<T>>(this).ReadAsync(cancellationToken);
                var path = GetPathToReference(externalReference.AssetName) + ".xnb";
                var container = await XnbContainer.ReadFromFileAsync(ContentRoot, path, cancellationToken);
                var asset = container.Asset;

                if (asset is not T typedAsset)
                {
                    throw new XnbFormatException($"Expected external reference {externalReference.AssetName} to be of type {typeof(T).FullName}, but got {asset?.GetType().FullName ?? "null"}");
                }

                return typedAsset;
            }

            if (!type.IsAssignableTo(typeof(T)))
            {
                throw new XnbFormatException($"Expected to read {typeof(T).FullName}, but got {type.FullName}");
            }

            return (T?)await ContentTypeMapper.Instance.GetReader(this, typeDefinition).ReadObjectAsync(cancellationToken);
        }

        public async ValueTask<T> ReadDirectAsync<T>(CancellationToken cancellationToken)
        {
            return await ContentTypeMapper.Instance.GetReader<T>(this).ReadAsync(cancellationToken);
        }

        public string GetPathToReference(string referenceName) => Path.Combine(ReferenceRoot, referenceName);
    }
}
