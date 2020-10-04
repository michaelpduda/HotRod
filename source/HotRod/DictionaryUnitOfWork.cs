using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace HotRod
{
    internal class DictionaryUnitOfWork<TIndex, TData> : IUnitOfWork<TIndex, TData>
    {
        private Func<IDictionary<string, string>> _retrieveDataCallback;
        private IDictionary<string, string> _currentItems;
        private Func<TIndex> _indexCreator;
        private Action<IDictionary<string, string>> _saveCallback;

        internal DictionaryUnitOfWork(
            Func<IDictionary<string, string>> retrieveDataCallback,
            Func<TIndex> indexCreator,
            Action<IDictionary<string, string>> saveCallback)
        {
            _retrieveDataCallback = retrieveDataCallback;
            Rollback();
            _indexCreator = indexCreator;
            _saveCallback = saveCallback;
        }

        public TData this[TIndex index]
        {
            get { return _currentItems[index.ToJson()].FromJson<TData>(); }
            set { _currentItems[index.ToJson()] = value.ToJson(); }
        }

        public int Count => _currentItems.Count;
        public bool IsReadOnly => false;
        public ICollection<TIndex> Keys => AsDictionary().Keys;
        public ICollection<TData> Values => AsDictionary().Values;

        public TIndex Add(TData newItem)
        {
            var index = _indexCreator();
            _currentItems[index.ToJson()] = newItem.ToJson();
            return index;
        }

        public void Add(TIndex key, TData value) => this[key] = value;

        public void Add(KeyValuePair<TIndex, TData> item) => this[item.Key] = item.Value;

        public void Clear() => _currentItems.Clear();

        public void Commit() => _saveCallback(_currentItems);

        public bool Contains(KeyValuePair<TIndex, TData> item) =>
            ContainsKey(item.Key) && (this[item.Key].ToJson() == item.Value.ToJson());

        public bool ContainsKey(TIndex key) => _currentItems.ContainsKey(key.ToJson());

        public void CopyTo(KeyValuePair<TIndex, TData>[] array, int arrayIndex) =>
            AsDictionary().ToArray().CopyTo(array, arrayIndex);

        public void Delete(TIndex index)
        {
            if (!ContainsKey(index))
                throw new ArgumentException($"The item with index {index.ToJson()} does not exist within the repository.");
            _currentItems.Remove(index.ToJson());
        }

        public IEnumerator<KeyValuePair<TIndex, TData>> GetEnumerator() =>
            AsDictionary().GetEnumerator();

        public void Rollback() =>
            _currentItems = _retrieveDataCallback();

        public bool Remove(TIndex key)
        {
            if (!ContainsKey(key))
                return false;
            try
            {
                _currentItems.Remove(key.ToJson());
                return true;
            }
            catch (Exception)
            { return false; }
        }

        public bool Remove(KeyValuePair<TIndex, TData> item)
        {
            if (!Contains(item))
                return false;
            try
            {
                Remove(item.Key);
                return true;
            }
            catch (Exception)
            { return false; }
        }

        public bool TryGetValue(TIndex key, [MaybeNullWhen(false)] out TData value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }
            value = default;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IDictionary<TIndex, TData> AsDictionary() =>
            _currentItems.ToDictionary(kv => kv.Key.FromJson<TIndex>(), kv => kv.Value.FromJson<TData>());
    }
}
