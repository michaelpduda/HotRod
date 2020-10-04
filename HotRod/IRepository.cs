using System;
using System.Collections.ObjectModel;

namespace HotRod
{
    public interface IRepository<TIndex, TData>
        where TIndex : struct
        where TData : struct
    {
        TData this[TIndex index] { get; }

        ReadOnlyDictionary<TIndex, TData> AllItems { get; }

        void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo);
    }
}
