
namespace CsToml.Formatter;

public sealed class IEnumerableFormatter<T> : CollectionBaseFormatter<IEnumerable<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IEnumerable<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}