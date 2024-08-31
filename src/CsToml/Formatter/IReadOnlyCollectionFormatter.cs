
namespace CsToml.Formatter;

internal sealed class IReadOnlyCollectionFormatter<T> : CollectionBaseFormatter<IReadOnlyCollection<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IReadOnlyCollection<T> Complete(List<T> collection)
    {
        return collection;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}