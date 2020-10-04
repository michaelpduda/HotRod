/* This file is part of the HotRod project, which is released under MIT License.
 * See LICENSE.md or visit:
 * https://github.com/michaelpduda/hotrod/blob/master/LICENSE.md
 */

using System;
using System.Collections.Generic;

namespace HotRod
{
    public interface IRepository<TIndex, TData> : IReadOnlyDictionary<TIndex, TData>
    {
        void StartWork(Action<IUnitOfWork<TIndex, TData>> workToDo);
    }
}
