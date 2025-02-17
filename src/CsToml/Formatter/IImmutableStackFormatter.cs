﻿using System.Collections.Immutable;

namespace CsToml.Formatter;

public sealed class IImmutableStackFormatter<T> : CollectionBaseFormatter<IImmutableStack<T>, T, List<T>>
{
    protected override void AddValue(List<T> mediator, T element)
    {
        mediator.Add(element);
    }

    protected override IImmutableStack<T> Complete(List<T> collection)
    {
        var stack = ImmutableStack<T>.Empty;
        for (var i = collection.Count - 1; i >= 0; i--)
        {
            stack = stack.Push(collection[i]);
        }
        return stack;
    }

    protected override List<T> CreateCollection(int capacity)
    {
        return new List<T>(capacity);
    }
}