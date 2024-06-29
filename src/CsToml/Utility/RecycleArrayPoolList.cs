
using System.Collections.Concurrent;

namespace CsToml.Utility;

internal sealed class RecycleArrayPoolList<T>
{
    private static readonly ConcurrentQueue<ArrayPoolList<T>> listQueue = new();

    private RecycleArrayPoolList() { }

    public static ArrayPoolList<T> Rent()
    {
        if (listQueue.TryDequeue(out var list))
        {
            return list;
        }
        return new ArrayPoolList<T>();
    }

    public static void Return(ArrayPoolList<T> list)
    {
        list.Clear();
        listQueue.Enqueue(list);
    }

    public static void Release()
    {
        while (listQueue.TryDequeue(out var list))
        {
            list.Dispose();
        }
    }
}