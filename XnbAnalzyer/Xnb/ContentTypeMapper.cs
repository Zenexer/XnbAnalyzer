using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XnbAnalzyer.Xnb.Reading;
using XnbAnalzyer.Xnb.Writing;

namespace XnbAnalzyer.Xnb
{
    public class ContentTypeMapper
    {
        private readonly Dictionary<string, Type> _writerTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _readerTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _contentTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> _xnaTypeNames = new Dictionary<string, string>();
        private readonly HashSet<string> _primitives = new HashSet<string>();

        public static ContentTypeMapper Instance { get; } = new ContentTypeMapper();

        public ContentTypeMapper()
        {
            var allTypes = typeof(ContentTypeMapper).Assembly.GetTypes();

            foreach (var type in allTypes.Where(t => t.IsClass && !t.IsAbstract && t.CustomAttributes.Any()))
            {
                foreach (var attribute in type.GetCustomAttributes(false))
                {
                    if (attribute is not ReaderWriterAttribute rw)
                    {
                        continue;
                    }

                    var (expectedBaseType, map) = rw switch
                    {
                        ReaderAttribute _ => (typeof(Reader<>), _readerTypes),
                        WriterAttribute _ => (typeof(Writer<>), _writerTypes),
                        _ => throw new Exception(),
                    };

                    var baseType = type.BaseType;
                    if (baseType?.GetGenericTypeDefinition() != expectedBaseType)
                    {
                        throw new Exception($"Expected reader to implement {expectedBaseType.FullName}");
                    }

                    map.Add(rw.TargetType, type);
                    map.Add(rw.TypeReaderName, type);

                    if (!_contentTypes.ContainsKey(rw.TargetType))
                    {
                        var contentType = baseType.GenericTypeArguments[0];
                        _contentTypes.Add(rw.TargetType, contentType);
                        _contentTypes.Add(rw.TypeReaderName, contentType);
                        _xnaTypeNames.Add(contentType.GetFullName(), rw.TargetType);

                        if (rw.IsPrimitive)
                        {
                            _primitives.Add(rw.TargetType);
                        }
                    }
                }
            }
        }

        public bool IsPrimitive(string targetType) => _primitives.Contains(targetType);
        public bool IsPrimitive<T>() => IsPrimitive(typeof(T).GetFullName());

        private Type ResolveGenerics(TypeDefinition definition, Type generic)
        {
            if (definition.TypeParameters.Length == 0)
            {
                return generic;
            }

            var typeParams = (from x in definition.TypeParameters
                              select GetContentType(x)).ToArray();

            return generic.MakeGenericType(typeParams);
        }

        private static Type ResolveGenerics<T>(Type generic)
        {
            if (!generic.IsGenericType)
            {
                return generic;
            }

            return generic.MakeGenericType(typeof(T).GenericTypeArguments);
        }

        private static object Instantiate<T>(T streamRw, Type rw)
            where T : class
        {
            var ctor = rw.GetConstructor(new[] { typeof(XnbStreamReader) }) ?? throw new Exception($"Reader or writer {rw.FullName} is missing valid constructor with parameter {typeof(T).FullName}");
            return ctor.Invoke(new object[] { streamRw });
        }

        public Type GetContentType(TypeDefinition definition) => ResolveGenerics(definition, _contentTypes[definition.Name]);
        public Type GetReaderType(TypeDefinition definition) => ResolveGenerics(definition, _readerTypes[definition.Name]);
        public Type GetWriterType(TypeDefinition definition) => ResolveGenerics(definition, _writerTypes[definition.Name]);

        public Type GetReaderType<T>() => ResolveGenerics<T>(_readerTypes[_xnaTypeNames[typeof(T).FullName ?? throw new Exception("Logic exception")]]);
        public Type GetWriterType<T>() => ResolveGenerics<T>(_writerTypes[_xnaTypeNames[typeof(T).FullName ?? throw new Exception("Logic exception")]]);

        public Reader CreateReader(XnbStreamReader rx, TypeDefinition definition) => (Reader)Instantiate(rx, GetReaderType(definition));
        public Writer CreateWriter(XnbStreamWriter tx, TypeDefinition definition) => (Writer)Instantiate(tx, GetWriterType(definition));

        public Reader<T> CreateReader<T>(XnbStreamReader rx) => (Reader<T>)Instantiate(rx, GetReaderType<T>());
        public Writer<T> CreateWriter<T>(XnbStreamWriter tx) => (Writer<T>)Instantiate(tx, GetWriterType<T>());
    }
}
