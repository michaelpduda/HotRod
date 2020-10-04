using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace HotRod
{
    public class JsonFileRepository<TIndex, TData> : IRepository<TIndex, TData>
    {
        private string _fileLocation;
        private Func<TIndex> _indexCreator;

        public JsonFileRepository(string fileLocation, Func<TIndex> indexCreator)
        {
            _indexCreator = indexCreator;
            _fileLocation = fileLocation;
        }

        public TData this[TIndex index] => ReadStrings()[index.ToJson()].FromJson<TData>();

        public int Count => ReadStrings().Count;
        public IEnumerable<TIndex> Keys => ReadDictionary().Keys;
        public IEnumerable<TData> Values => ReadDictionary().Values;

        public bool ContainsKey(TIndex key) => ReadStrings().ContainsKey(key.ToJson());

        public IEnumerator<KeyValuePair<TIndex, TData>> GetEnumerator() => ReadDictionary().GetEnumerator();

        public void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo)
        {
            lock (_fileLocation)
            {
                workToDo(new DictionaryUnitOfWork<TIndex, TData>(
                    ReadStrings,
                    _indexCreator,
                    d => File.WriteAllText(_fileLocation, JsonConvert.SerializeObject(d, Formatting.Indented))));
            }
        }

        public bool TryGetValue(TIndex key, [MaybeNullWhen(false)] out TData value)
        {
            var strings = ReadStrings();
            if (strings.TryGetValue(key.ToJson(), out string json))
            {
                value = json.FromJson<TData>();
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IDictionary<TIndex, TData> ReadDictionary() =>
            ReadStrings().ToDictionary(kv => kv.Key.FromJson<TIndex>(), kv => kv.Value.FromJson<TData>());

        private IDictionary<string, string> ReadStrings() =>
            File.Exists(_fileLocation)
                ? File.ReadAllText(_fileLocation).FromJson<IDictionary<string, string>>()
                : new Dictionary<string, string>();
    }
}
