using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Chatly.Desktop.Models.UserSession;

public abstract class ObservableCollectionState<TItem> : ObservableObject
    where TItem : class, IIdentifiable
{
    private readonly ObservableCollection<TItem> _items = [];

    protected ObservableCollectionState()
    {
        Items = new ReadOnlyObservableCollection<TItem>(_items);
    }

    public ReadOnlyObservableCollection<TItem> Items { get; }

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => _items.CollectionChanged += value;
        remove => _items.CollectionChanged -= value;
    }

    protected bool AddItem(TItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var existing = _items.FirstOrDefault(current => current.Id == item.Id);
        if (existing is not null)
        {
            OnExistingItem(existing, item);
            return false;
        }

        _items.Add(item);
        OnItemAdded(item);
        return true;
    }

    protected void SetItems(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var replacements = items.DistinctBy(item => item.Id).ToList();
        var replacementIds = replacements.Select(item => item.Id).ToHashSet();

        foreach (var stale in _items.Where(item => !replacementIds.Contains(item.Id)).ToList())
        {
            RemoveItem(stale);
        }

        for (var index = 0; index < replacements.Count; index++)
        {
            var id = replacements[index].Id;
            AddItem(replacements[index]);

            var currentIndex = _items.IndexOf(_items.First(item => item.Id == id));
            if (currentIndex != index)
            {
                _items.Move(currentIndex, index);
            }
        }
    }

    protected bool RemoveItem(Guid id)
    {
        var item = _items.FirstOrDefault(current => current.Id == id);
        return item is not null && RemoveItem(item);
    }

    protected bool RemoveItem(TItem item)
    {
        if (!_items.Contains(item))
        {
            return false;
        }

        OnItemRemoving(item);
        return _items.Remove(item);
    }

    internal void Clear()
    {
        ClearItems();
        OnCleared();
    }

    protected virtual void OnExistingItem(TItem existing, TItem incoming)
    {
    }

    protected virtual void OnItemAdded(TItem item)
    {
    }

    protected virtual void OnItemRemoving(TItem item)
    {
    }

    protected virtual void OnItemsClearing()
    {
    }

    protected virtual void OnCleared()
    {
    }

    private void ClearItems()
    {
        OnItemsClearing();
        _items.Clear();
    }
}