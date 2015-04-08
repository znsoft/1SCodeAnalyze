using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace V8Reader.Core
{
    class MDObjectsCollection<TItem> : IEnumerable<TItem>
    {

        public MDObjectsCollection()
        {
            m_Collection = new List<TItem>();
        }

        public TItem this[int index]
        {
            get { return m_Collection[index]; }
        }

        public void Add(TItem item)
        {
            m_Collection.Add(item);
        }

        public void RemoveAt(int index)
        {
            m_Collection.RemoveAt(index);
        }

        public void Clear()
        {
            m_Collection.Clear();
        }

        public int Count
        {
            get { return m_Collection.Count; }
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return new MDObjectsEnumerator<TItem>(m_Collection);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)GetEnumerator();
        }

        private List<TItem> m_Collection;

    }

    class MDObjectsEnumerator<T> : IEnumerator<T>
    {
        private List<T> m_collection;
        private int m_CurrentIndex;
        private T m_current;

        public MDObjectsEnumerator(List<T> Collection)
        {
            m_collection = Collection;
            Reset();
        }

        public T Current
        {
            get { return m_current; }
        }

        public void Dispose()
        {

        }

        object System.Collections.IEnumerator.Current
        {
            get { return m_current; }
        }

        public bool MoveNext()
        {
            if (++m_CurrentIndex >= m_collection.Count)
            {
                return false;
            }
            else
            {
                m_current = m_collection[m_CurrentIndex];
                return true;
            }
        }

        public void Reset()
        {
            m_CurrentIndex = -1;
            m_current = default(T);
        }
    }

}