/* This file is part of the HotRod project, which is released under MIT License.
 * See LICENSE.md or visit:
 * https://github.com/michaelpduda/hotrod/blob/master/LICENSE.md
 */

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
