using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalzyer.Xnb.Content;
using XnbAnalzyer.Xnb.Reading;

namespace XnbAnalzyer.Xnb
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

            return ContentTypeMapper.Instance.CreateReader(this, typeDefinition).ReadObject();
        }

        public T? Read<T>()
        {
            if (ContentTypeMapper.Instance.IsPrimitive<T>())
            {
                return ContentTypeMapper.Instance.CreateReader<T>(this).Read();
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

            return (T)ContentTypeMapper.Instance.CreateReader(this, typeDefinition).ReadObject();
        }
    }
}
