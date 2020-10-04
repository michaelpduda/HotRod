namespace HotRod
{
    public interface IUnitOfWork<TIndex, TData>
        where TIndex : struct
    {
        TData this[TIndex index] { get; set; }

        TIndex Add(TData newItem);
        void Delete(TIndex index);
        void SaveState();
    }
}
