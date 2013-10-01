using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetCover
{
    public class NodeChildren<T> : IList<T>
    {
        private List<T> list = new List<T>();
        private int memberCount = 0;

        public int ChildCount
        {
            get { return memberCount; }
        }

        public NodeChildren()
        {
        }

        public T Get(int i)
        {
            return list[i];
        }

        public void Sort()
        {
            list.Sort();
        }

        public NodeChildren(HashSet<T> inlist)
        {
            this.list = new List<T>(inlist);
            memberCount = this.list.Count;
        }

        /// <summary>
        /// Ensures there are no duplicate adds
        /// </summary>
        /// <param name="node"></param>
        public void Add(T node)
        {
            if (!this.list.Contains(node))
            {
                this.list.Add(node);
                memberCount++;
            }

        }

        /// <summary>
        /// Ensures removal is smooth
        /// </summary>
        /// <param name="node"></param>
        public void RemoveChild(T node)
        {
            if (this.list.Contains(node))
            {
                this.list.Remove(node);
                memberCount--;
            }
        }


        public int IndexOf(T item)
        {
           return this.list.IndexOf(item); 
        }

        public void Insert(int index, T item)
        {
            if (!this.list.Contains(item))
            {
                this.list.Insert(index, item);
                memberCount++;
            }

        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public T this[int index]
        {
            get
            {
                return this.list[index];
            }
            set
            {
                this.list[index] = value;
            }
        }


        public void Clear()
        {
            this.list.Clear(); 
        }

        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex); 
        }

        public int Count
        {
            get { return this.list.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return this.list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
