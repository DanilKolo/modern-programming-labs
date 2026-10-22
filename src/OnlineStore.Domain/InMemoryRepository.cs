using System;
using System.Collections.Generic;

namespace OnlineStore.Domain;

public class InMemoryRepository<T>
{
    private readonly List<T> _items = [];

    public void Add(T item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
    }

    public void AddRange(IEnumerable<T> items)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        _items.AddRange(items);
    }

    public bool Remove(T item) => _items.Remove(item);

    public IReadOnlyCollection<T> GetAll() => _items.AsReadOnly();
}