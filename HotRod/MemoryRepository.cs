using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace HotRod
{
    public class MemoryRepository<TIndex, TData> : IRepository<TIndex, TData>
        where TIndex : struct
    {
        private Func<TIndex> _indexCreator;
        private IDictionary<TIndex, string> _savedItems = new Dictionary<TIndex, string>();

        public MemoryRepository(Func<TIndex> indexCreator)
        {
            _indexCreator = indexCreator;
        }

        public TData this[TIndex index]
        {
            get { return JsonConvert.DeserializeObject<TData>(_savedItems[index]); }
        }

        public ReadOnlyDictionary<TIndex, TData> AllItems
        {
            get { return new ReadOnlyDictionary<TIndex, TData>(_savedItems.ToDictionary(
                    (KeyValuePair<TIndex, string> kv) => kv.Key,
                    (KeyValuePair<TIndex, string> kv) => JsonConvert.DeserializeObject<TData>(kv.Value))); }
        }

        public void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo)
        {
            lock (_savedItems)
            {
                workToDo(new MemoryUnitOfWork(
                    _savedItems.ToDictionary(
                        kv => kv.Key,
                        kv => JsonConvert.DeserializeObject<TData>(kv.Value)),
                    _indexCreator,
                    d => _savedItems = d.ToDictionary(
                        kv => kv.Key,
                        kv => JsonConvert.SerializeObject(kv.Value))));
            }
        }

        private class MemoryUnitOfWork : IUnitOfWork<TIndex, TData>
        {
            private IDictionary<TIndex, TData> _currentItems;
            private Func<TIndex> _indexCreator;
            private Action<IDictionary<TIndex, TData>> _saveCallback;

            internal MemoryUnitOfWork(
                IDictionary<TIndex, TData> currentItems,
                Func<TIndex> indexCreator,
                Action<IDictionary<TIndex, TData>> saveCallback)
            {
                _currentItems = currentItems;
                _indexCreator = indexCreator;
                _saveCallback = saveCallback;
            }

            public TData this[TIndex index]
            {
                get { return _currentItems[index]; }
                set
                {
                    if (!_currentItems.ContainsKey(index))
                        throw new ArgumentException($"The item with index {index.ToString()} does not exist within the repository.");
                    _currentItems[index] = value;
                }
            }

            public TIndex Add(TData newItem)
            {
                var index = _indexCreator();
                _currentItems[index] = newItem;
                return index;
            }

            public void Delete(TIndex index)
            {
                if (!_currentItems.ContainsKey(index))
                    throw new ArgumentException($"The item with index {index.ToString()} does not exist within the repository.");
                _currentItems.Remove(index);
            }

            public void SaveState()
                => _saveCallback(_currentItems);
        }
    }
}
