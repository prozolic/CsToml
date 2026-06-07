
namespace CsToml.Utility;

internal ref struct TempList<T>
{
    private T[]? items;
    private Span<T> itemsSpan;
    private int count;

    public T this[int index] => itemsSpan[index];

    public ReadOnlySpan<T> Items => itemsSpan.Slice(0, count);

    public int Count => count;

    public TempList(Span<T> items)
    {
        this.items = null;
        this.itemsSpan = items;
        count = 0;
    }

    public void Add(T item)
    {
        if (count == itemsSpan.Length)
        {
            EnsureCapacity();
        }
        itemsSpan[count++] = item;
    }

    public void RemoveLast()
    {
        if (count > 0)
        {
            count--;
        }
    }

    public void RemoveLastIfFound(T key)
    {
        if (count > 0 && (itemsSpan[count - 1]?.Equals(key) ?? false))
        {
            count--;
        }
    }

    public void Clear()
    {
        count = 0;
    }

    private void EnsureCapacity()
    {
        var newSize = itemsSpan.Length * 2;
        items = new T[newSize];
        itemsSpan.CopyTo(items.AsSpan(0, itemsSpan.Length));
        itemsSpan = items;
    }
}
