
namespace CsToml.Formatter;

public sealed class StackFormatter<T> : CollectionBaseFormatter<Stack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override Stack<T> Complete(List<T> collection)
    {
        var stack = new Stack<T>(collection.Count);
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            stack.Push(collection[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}
