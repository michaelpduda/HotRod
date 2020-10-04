using System.Collections.Generic;

namespace HotRod
{
    public interface IUnitOfWork<TIndex, TData> : IDictionary<TIndex, TData>
    {
        TIndex Add(TData newItem);
        void Commit();
        void Rollback();
    }
}
