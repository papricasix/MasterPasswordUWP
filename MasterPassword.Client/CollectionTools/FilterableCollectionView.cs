using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MasterPasswordUWP.CollectionTools
{
    public class FilterableCollectionView<T> : IList, INotifyCollectionChanged//, IItemsRangeInfo, ISelectionInfo
    {
        public delegate string OrderByProperty();

        public delegate object OrderFunc(T a);
        public delegate bool IncludedFunc(T a);

        private IEnumerable<T> _source;
        private List<T> _view;

        public IEnumerable<T> Source
        {
            get { return _source; }
            set { _source = /*new ObservableCollection<T>*/(value); RefreshView(); }
        }

        private OrderByProperty _orderBy;

        public OrderByProperty OrderBy
        {
            get { return _orderBy; }
            set { _orderBy = value; CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); }
        }

        public bool AscendingOrder { get; set; }

        private OrderFunc _orderFunc;

        public OrderFunc OrderFunction
        {
            get { return _orderFunc; }
            set { _orderFunc = value; RefreshView(); }
        }

        private IncludedFunc _includeFunc;

        public IncludedFunc IncludedFunction
        {
            get { return _includeFunc; }
            set { _includeFunc = value; RefreshView(); }
        }

        public FilterableCollectionView() : this(null)
        {
        }

        public FilterableCollectionView(IEnumerable<T> source)
        {
            _source = source ?? new T[0];
            _view = new List<T>(Source);
        }

        #region IList


        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable) _view).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection) _view).CopyTo( array, index );
        }

        public int Count
        {
            get { return ((ICollection) _view).Count; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection) _view).IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return ((ICollection) _view).SyncRoot; }
        }

        public int Add(object value)
        {
            return ((IList) _view).Add( value );
        }

        public void Clear()
        {
            ((IList) _view).Clear();
        }

        public bool Contains(object value)
        {
            return ((IList) _view).Contains( value );
        }

        public int IndexOf(object value)
        {
            return ((IList) _view).IndexOf( value );
        }

        public void Insert(int index, object value)
        {
            ((IList) _view).Insert( index, value );
        }

        public void Remove(object value)
        {
            ((IList) _view).Remove( value );
        }

        public void RemoveAt(int index)
        {
            ((IList) _view).RemoveAt( index );
        }

        public bool IsFixedSize
        {
            get { return ((IList) _view).IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return ((IList) _view).IsReadOnly; }
        }

        public object this[int index]
        {
            get { return ((IList) _view)[index]; }
            set { ((IList) _view)[index] = value; }
        }

        #endregion

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged
        /*{
            add { FilteredCollection.CollectionChanged += value; }
            remove { FilteredCollection.CollectionChanged -= value; }
        }*/;

        #endregion

        public void RefreshView()
        {
            // filtering
            var newView = _source.Where( e => IncludedFunction?.Invoke( e ) ?? true );

            // sorting
            if ( AscendingOrder )
            {
                newView = newView.OrderBy( e => OrderFunction?.Invoke( e ) );
            }
            else
            {
                newView = newView.OrderByDescending( e => OrderFunction?.Invoke( e ) );
            }

            _view.Clear();
            _view.AddRange(newView);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
