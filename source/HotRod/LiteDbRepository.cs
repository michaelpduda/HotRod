/* This file is part of the HotRod project, which is released under MIT License.
 * See LICENSE.md or visit:
 * https://github.com/michaelpduda/hotrod/blob/master/LICENSE.md
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LiteDB;

namespace HotRod
{
    public class LiteDbRepository<TIndex, TData> : IRepository<TIndex, TData>
    {
        private string _fileLocation;
        private Func<TIndex> _indexCreator;
        private string _indexName;

        static LiteDbRepository() => BsonMapper.Global.EmptyStringToNull = false;

        public LiteDbRepository(string fileLocation, string indexName, Func<TIndex> indexCreator)
        {
            _fileLocation = fileLocation;
            _indexCreator = indexCreator;
            _indexName = indexName;
        }

        public TData this[TIndex key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"The given key '{key.ToJson()}' was not present in the dictionary.");

        public int Count => OpenDb(collection => collection.Query().Count());
        public IEnumerable<TIndex> Keys => OpenDb(collection => collection.Query().Select(document => document.Id.FromJson<TIndex>()).ToEnumerable());
        public IEnumerable<TData> Values => OpenDb(collection => collection.Query().Select(document => document.Value).ToEnumerable());

        public bool ContainsKey(TIndex key) =>
            OpenDb(collection => collection.FindById(key.ToJson()) != null);

        public IEnumerator<KeyValuePair<TIndex, TData>> GetEnumerator() =>
            OpenDb(collection => collection.Query().ToList()).Select(document => new KeyValuePair<TIndex, TData>(document.Id.FromJson<TIndex>(), document.Value)).GetEnumerator();

        public void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo)
        {
            using var liteDb = new LiteDatabase(_fileLocation);
            var collection = liteDb.GetCollection<RepositoryDocument>(_indexName);
            liteDb.BeginTrans();
            workToDo(new LiteDbUnitOfWork(
                collection,
                () =>
                {
                    liteDb.Commit();
                    liteDb.BeginTrans();
                },
                () => liteDb.Rollback(),
                _indexCreator));
            liteDb.Rollback();
        }

        public bool TryGetValue(TIndex key, [MaybeNullWhen(false)] out TData value)
        {
            var result = OpenDb(collection => collection.FindById(key.ToJson()));
            if (result == null)
            {
                value = default;
                return false;
            }
            value = result.Value;
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private TResult OpenDb<TResult>(Func<ILiteCollection<RepositoryDocument>, TResult> retriever)
        {
            using var liteDb = new LiteDatabase(_fileLocation);
            return retriever(liteDb.GetCollection<RepositoryDocument>(_indexName));
        }

        private class RepositoryDocument
        {
            public string Id { get; set; }
            public TData Value { get; set; }
        }

        private class LiteDbUnitOfWork : IUnitOfWork<TIndex, TData>
        {
            private ILiteCollection<RepositoryDocument> _collection;
            private Func<TIndex> _indexCreator;
            private Action _saveCallback;
            private Action _rollbackCallback;

            public LiteDbUnitOfWork(ILiteCollection<RepositoryDocument> collection, Action saveCallback, Action rollbackCallback, Func<TIndex> indexCreator)
            {
                _collection = collection;
                _indexCreator = indexCreator;
                _saveCallback = saveCallback;
                _rollbackCallback = rollbackCallback;
                Keys = new KeyCollection(this);
                Values = new ValueCollection(this);
            }

            public TData this[TIndex key]
            {
                get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"The given key '{key.ToJson()}' was not present in the dictionary.");
                set
                {
                    var document = _collection.FindById(key.ToJson()) ?? new RepositoryDocument { Id = key.ToJson() };
                    document.Value = value;
                    _collection.Upsert(document);
                }
            }

            public int Count => _collection.Query().Count();
            public bool IsReadOnly => false;
            public ICollection<TIndex> Keys { get; }
            public ICollection<TData> Values { get; }

            public TIndex Add(TData newItem)
            {
                var id = _indexCreator();
                var document = new RepositoryDocument
                {
                    Id = id.ToJson(),
                    Value = newItem,
                };
                _collection.Insert(document);
                return id;
            }

            public void Add(TIndex key, TData value) => this[key] = value;

            public void Add(KeyValuePair<TIndex, TData> item) => this[item.Key] = item.Value;

            public void Clear() => _collection.DeleteAll();

            public void Commit() => _saveCallback();

            public bool Contains(KeyValuePair<TIndex, TData> item)
            {
                var json = item.Key.ToJson();
                return _collection.FindOne(document => document.Id == json && (document.Value.ToJson() == item.Value.ToJson())) != null;
            }

            public bool ContainsKey(TIndex key) => _collection.FindById(key.ToJson()) != null;

            public void CopyTo(KeyValuePair<TIndex, TData>[] array, int arrayIndex) =>
                _collection.Query().Select(document => new KeyValuePair<string, TData>(document.Id, document.Value)).ToArray().CopyTo(array, arrayIndex);

            public IEnumerator<KeyValuePair<TIndex, TData>> GetEnumerator() =>
                _collection.Query().Select(document => new KeyValuePair<TIndex, TData>(document.Id.FromJson<TIndex>(), document.Value)).ToEnumerable().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Remove(TIndex key) =>
                _collection.Delete(key.ToJson());

            public bool Remove(KeyValuePair<TIndex, TData> item)
            {
                var json = item.Key.ToJson();
                var result = _collection.FindOne(document => document.Id == json && (document.Value.ToJson() == item.Value.ToJson()));
                if (result is null)
                    return false;
                try
                {
                    Remove(item.Key);
                    return true;
                }
                catch (Exception e)
                { return false; }
            }

            public void Rollback() =>
                _rollbackCallback();

            public bool TryGetValue(TIndex key, [MaybeNullWhen(false)] out TData value)
            {
                var result = _collection.FindById(key.ToJson());
                if (result == null)
                {
                    value = default;
                    return false;
                }
                value = result.Value;
                return true;
            }

            private sealed class KeyCollection : ICollection<TIndex>
            {
                private LiteDbUnitOfWork _unitOfWork;

                public KeyCollection(LiteDbUnitOfWork unitOfWork) =>
                    _unitOfWork = unitOfWork;

                public int Count => _unitOfWork.Count;

                public bool IsReadOnly => true;

                public void Add(TIndex item) => throw new NotSupportedException();

                public void Clear() => throw new NotSupportedException();

                public bool Contains(TIndex item) => _unitOfWork.ContainsKey(item);

                public void CopyTo(TIndex[] array, int arrayIndex) =>
                    _unitOfWork._collection.Query().Select(doc => doc.Id.FromJson<TIndex>()).ToArray().CopyTo(array, arrayIndex);

                public IEnumerator<TIndex> GetEnumerator() =>
                    _unitOfWork._collection.Query().Select(doc => doc.Id.FromJson<TIndex>()).ToEnumerable().GetEnumerator();

                public bool Remove(TIndex item) => throw new NotSupportedException();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            private sealed class ValueCollection : ICollection<TData>
            {
                private LiteDbUnitOfWork _unitOfWork;

                public ValueCollection(LiteDbUnitOfWork unitOfWork) =>
                    _unitOfWork = unitOfWork;

                public int Count => _unitOfWork.Count;

                public bool IsReadOnly => true;

                public void Add(TData item) => throw new NotSupportedException();

                public void Clear() => throw new NotSupportedException();

                public bool Contains(TData item) =>
                    _unitOfWork._collection.Query().Where(doc => doc.Value.Equals(item)).Count() > 0;

                public void CopyTo(TData[] array, int arrayIndex) =>
                    _unitOfWork._collection.Query().Select(doc => doc.Value).ToArray().CopyTo(array, arrayIndex);

                public IEnumerator<TData> GetEnumerator() =>
                    _unitOfWork._collection.Query().Select(doc => doc.Value).ToEnumerable().GetEnumerator();

                public bool Remove(TData item) => throw new NotSupportedException();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }
    }
}
