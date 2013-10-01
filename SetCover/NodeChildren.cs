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

    }
}
