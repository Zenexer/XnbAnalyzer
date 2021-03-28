using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XnbAnalyzer.Xnb.Reading;
using XnbAnalyzer.Xnb.Writing;

namespace XnbAnalyzer.Xnb
{
    public class ContentTypeMapper
    {
        private readonly Dictionary<string, Type> _writerTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _readerTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, Type> _contentTypes = new Dictionary<string, Type>();
        private readonly Dictionary<string, string> _xnaTypeNames = new Dictionary<string, string>();
        private readonly HashSet<string> _primitives = new HashSet<string>();

        private readonly ConditionalWeakTable<XnbStreamReader, ConditionalWeakTable<TypeDefinition, Reader>> _readers = new ConditionalWeakTable<XnbStreamReader, ConditionalWeakTable<TypeDefinition, Reader>>();
        private readonly ConditionalWeakTable<XnbStreamWriter, ConditionalWeakTable<TypeDefinition, Writer>> _writers = new ConditionalWeakTable<XnbStreamWriter, ConditionalWeakTable<TypeDefinition, Writer>>();
        private readonly ConditionalWeakTable<XnbStreamReader, ConditionalWeakTable<Type, Reader>> _genericReaders = new ConditionalWeakTable<XnbStreamReader, ConditionalWeakTable<Type, Reader>>();
        private readonly ConditionalWeakTable<XnbStreamWriter, ConditionalWeakTable<Type, Writer>> _genericWriters = new ConditionalWeakTable<XnbStreamWriter, ConditionalWeakTable<Type, Writer>>();
        private readonly ConditionalWeakTable<object, ConditionalWeakTable<Type, object>> _instances = new ConditionalWeakTable<object, ConditionalWeakTable<Type, object>>();

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

                    var generic = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                    map.Add(rw.TargetType, generic);
                    map.Add(rw.TypeReaderName, generic);

                    if (!_contentTypes.ContainsKey(rw.TargetType))
                    {
                        var contentType = baseType.GenericTypeArguments[0];
                        if (contentType.IsGenericType)
                        {
                            contentType = contentType.GetGenericTypeDefinition();
                        }

                        var contentTypeName = contentType.GetFullName();

                        _contentTypes.Add(rw.TargetType, contentType);
                        _contentTypes.Add(rw.TypeReaderName, contentType);
                        _xnaTypeNames.Add(contentTypeName, rw.TargetType);

                        if (contentTypeName != rw.TargetType)
                        {
                            _contentTypes.Add(contentTypeName, contentType);
                            map.Add(contentTypeName, contentType);
                        }

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
            definition = definition.Resolved;

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

        private object GetOrInstantiate<T>(T streamRw, Type rw)
            where T : class
            => _instances.GetOrCreateValue(streamRw).GetValue(rw, _ =>
                rw.GetConstructor(new[] { typeof(T) })?.Invoke(new object[] { streamRw }) ?? throw new Exception($"Reader or writer {rw.FullName} is missing valid constructor with parameter {typeof(T).FullName}")
            );

        public Type GetContentType(TypeDefinition definition) => ResolveGenerics(definition, _contentTypes[definition.Resolved.Name]);
        public Type GetReaderType(TypeDefinition definition) => ResolveGenerics(definition, _readerTypes[definition.Resolved.Name]);
        public Type GetWriterType(TypeDefinition definition) => ResolveGenerics(definition, _writerTypes[definition.Resolved.Name]);

        public Type GetWriterType<T>() => ResolveGenerics<T>(_writerTypes[_xnaTypeNames[typeof(T).GetGenericName()]]);
        public Type GetReaderType<T>() => ResolveGenerics<T>(_readerTypes[_xnaTypeNames[typeof(T).GetGenericName()]]);

        public Reader GetReader(XnbStreamReader rx, TypeDefinition definition) => _readers.GetOrCreateValue(rx).GetValue(definition, _ => (Reader)GetOrInstantiate(rx, GetReaderType(definition)));
        public Writer GetWriter(XnbStreamWriter tx, TypeDefinition definition) => _writers.GetOrCreateValue(tx).GetValue(definition, _ => (Writer)GetOrInstantiate(tx, GetWriterType(definition)));

        public Reader<T> GetReader<T>(XnbStreamReader rx) => (Reader<T>)_genericReaders.GetOrCreateValue(rx).GetValue(typeof(T), _ => (Reader)GetOrInstantiate(rx, GetReaderType<T>()));
        public Writer<T> GetWriter<T>(XnbStreamWriter tx) => (Writer<T>)_genericWriters.GetOrCreateValue(tx).GetValue(typeof(T), _ => (Writer)GetOrInstantiate(tx, GetWriterType<T>()));
    }
}
