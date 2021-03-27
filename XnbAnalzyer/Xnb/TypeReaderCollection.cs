using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XnbAnalzyer.Xnb
{
    public class TypeReaderCollection : IReadOnlyList<TypeDefinition>
    {
        private readonly ImmutableArray<TypeDefinition> _items;

        public TypeReaderCollection(IEnumerable<TypeDefinition> entries) => _items = entries.ToImmutableArray();

        public TypeDefinition this[int index] => _items[index];

        public int Count => _items.Length;

        public IEnumerator<TypeDefinition> GetEnumerator() => ((IEnumerable<TypeDefinition>)_items).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();
    }
}
