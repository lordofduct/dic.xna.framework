using System;
using System.Collections.Generic;
using System.Linq;

namespace Dic.Xna.Framework.Collections
{
    public class UniqueList<T> : IList<T>, System.Collections.ICollection
    {

        #region Fields

        private IList<T> _lst;
        private IEqualityComparer<T> _comparer;

        #endregion

        #region CONSTRUCTOR

        public UniqueList()
            : this((null as IEqualityComparer<T>))
        {
            
        }

        public UniqueList(int capacity)
            : this(capacity, null)
        {

        }

        public UniqueList(IEnumerable<T> collection)
            : this(collection, null)
        {

        }

        public UniqueList(IList<T> lst, bool bWrap = false)
            : this(lst, bWrap, null)
        {

        }

        public UniqueList(IEqualityComparer<T> comparer)
        {
            _lst = new List<T>();
            _comparer = (comparer == null) ? EqualityComparer<T>.Default : comparer;
        }

        public UniqueList(int capacity, IEqualityComparer<T> comparer)
        {
            _lst = new List<T>(capacity);
            _comparer = (comparer == null) ? EqualityComparer<T>.Default : comparer;
        }

        public UniqueList(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _lst = new List<T>();
            _comparer = (comparer == null) ? EqualityComparer<T>.Default : comparer;

            this.AddRange(collection);
        }

        public UniqueList(IList<T> lst, bool bWrap, IEqualityComparer<T> comparer)
        {
            _comparer = (comparer == null) ? EqualityComparer<T>.Default : comparer;
            if (bWrap)
            {
                _lst = lst;
            }
            else
            {
                _lst = new List<T>();
                this.AddRange(lst);
            }
        }

        #endregion

        #region Methods

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var obj in collection)
            {

            }
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<T> AsReadOnly()
        {
            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(_lst);
        }

        public T[] ToArray()
        {
            return _lst.ToArray();
        }

        /// <summary>
        /// Removes any duplicates in a wrapped IList that may have creeped in.
        /// </summary>
        public void Clean()
        {
            for (int i = 0; i < _lst.Count; i++)
            {
                for (int j = i + 1; j < _lst.Count; j++)
                {
                    if (_comparer.Equals(_lst[i], _lst[j]))
                    {
                        _lst.RemoveAt(j);
                    }
                }
            }
        }

        public void RemoveAll(Predicate<T> func)
        {
            if (_lst is List<T>)
                (_lst as List<T>).RemoveAll(func);
            else
            {
                var arr = _lst.ToArray();
                foreach (var obj in arr)
                {
                    if (func(obj)) _lst.Remove(obj);
                }
            }
        }

        #endregion

        #region IList Interface

        public int IndexOf(T item)
        {
            int cnt = _lst.Count;
            for (int i = 0; i < cnt; i++)
            {
                if (_comparer.Equals(item, _lst[i])) return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            if (this.Contains(item))
            {
                if(this.IndexOf(item) < index) index--;
                this.Remove(item);
            }

            _lst.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _lst.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return _lst[index];
            }
            set
            {
                if (this.Contains(value) && _comparer.Equals(_lst[index], value))
                {
                    var i = this.IndexOf(value);
                    if (i < index) index--;
                    _lst.RemoveAt(i);
                }

                _lst[index] = value;
            }
        }

        public void Add(T item)
        {
            if (this.Contains(item)) this.Remove(item);
            _lst.Add(item);
        }

        public void Clear()
        {
            _lst.Clear();
        }

        public bool Contains(T item)
        {
            foreach (var obj in _lst)
            {
                if (_comparer.Equals(obj, item)) return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _lst.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _lst.Count; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return _lst.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return _lst.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _lst.GetEnumerator();
        }

        #endregion

        #region ICollection Interface

        void System.Collections.ICollection.CopyTo(Array array, int index)
        {
            if (_lst is System.Collections.ICollection)
            {
                (_lst as System.Collections.ICollection).CopyTo(array, index);
            }
            else
            {
                _lst.ToArray().CopyTo(array, index);
            }
        }

        bool System.Collections.ICollection.IsSynchronized
        {
            get { return (_lst is System.Collections.ICollection) ? (_lst as System.Collections.ICollection).IsSynchronized : false; }
        }

        object System.Collections.ICollection.SyncRoot
        {
            get { return (_lst is System.Collections.ICollection) ? (_lst as System.Collections.ICollection).SyncRoot : _lst; }
        }

        #endregion

    }
}
