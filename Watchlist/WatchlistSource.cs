using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace Watchlist
{
    class WatchlistSource : List<Stock>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public new void Add(Stock item)
        {
            base.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public new void Insert(int index, Stock item)
        {
            base.Insert(index, item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }
    }
}
