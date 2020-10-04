using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Newtonsoft.Json;

namespace HotRod
{
    public class MemoryRepository<TIndex, TData> : IRepository<TIndex, TData>
    {
        private Func<TIndex> _indexCreator;
        private IDictionary<string, string> _savedItems = new Dictionary<string, string>();

        public MemoryRepository(Func<TIndex> indexCreator)
        {
            _indexCreator = indexCreator;
        }

        public TData this[TIndex index] => _savedItems[index.ToJson()].FromJson<TData>();

        public int Count => _savedItems.Count;
        public IEnumerable<TIndex> Keys => _savedItems.Keys.Select(json => json.FromJson<TIndex>());
        public IEnumerable<TData> Values => _savedItems.Values.Select(json => json.FromJson<TData>());

        public bool ContainsKey(TIndex key) => _savedItems.ContainsKey(key.ToJson());

        public IEnumerator<KeyValuePair<TIndex, TData>> GetEnumerator() =>
            _savedItems.Select(kv => new KeyValuePair<TIndex, TData>(kv.Key.FromJson<TIndex>(), kv.Value.FromJson<TData>())).GetEnumerator();

        public void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo)
        {
            lock (_savedItems)
            {
                workToDo(new DictionaryUnitOfWork<TIndex, TData>(
                    () => _savedItems.ToDictionary(kv => kv.Key, kv => kv.Value),
                    _indexCreator,
                    d => _savedItems = d.ToDictionary(kv => kv.Key, kv => kv.Value)));
            }
        }

        public bool TryGetValue(TIndex key, [MaybeNullWhen(false)] out TData value)
        {
            if (_savedItems.TryGetValue(key.ToJson(), out string json))
            {
                value = json.FromJson<TData>();
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
