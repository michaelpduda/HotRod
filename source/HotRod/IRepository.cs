using System;
using System.Collections.Generic;

namespace HotRod
{
    public interface IRepository<TIndex, TData> : IReadOnlyDictionary<TIndex, TData>
    {
        void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo);
    }
}
