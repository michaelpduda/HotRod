using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;

namespace HotRod
{
    public class JsonFileRepository<TIndex, TData> : IRepository<TIndex, TData>
        where TIndex : struct
    {
        private string _fileLocation;
        private Func<TIndex> _indexCreator;

        public JsonFileRepository(string fileLocation, Func<TIndex> indexCreator)
        {
            _indexCreator = indexCreator;
            _fileLocation = fileLocation;
        }

        public TData this[TIndex index]
        {
            get { return ReadFile()[index]; }
        }

        public ReadOnlyDictionary<TIndex, TData> AllItems
        {
            get { return new ReadOnlyDictionary<TIndex, TData>(ReadFile()); }
        }

        public void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo)
        {
            lock (_fileLocation)
            {
                workToDo(new MemoryUnitOfWork(
                    ReadFile(),
                    _indexCreator,
                    d => File.WriteAllText(_fileLocation, JsonConvert.SerializeObject(d, Formatting.Indented))));
            }
        }

        private IDictionary<TIndex, TData> ReadFile()
            => File.Exists(_fileLocation)
                ? JsonConvert.DeserializeObject<IDictionary<TIndex, TData>>(File.ReadAllText(_fileLocation))
                : new Dictionary<TIndex, TData>();

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
