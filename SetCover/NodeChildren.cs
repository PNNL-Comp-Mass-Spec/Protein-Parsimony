using System;
using System.Collections.Generic;

namespace SetCover
{
	public class NodeChildren<T> : IList<T>
	{
		private readonly List<T> Nodes = new List<T>();

		private int memberCount;

		public int ChildCount => memberCount;

	    public NodeChildren()
		{
		}

		public T Get(int i)
		{
			return Nodes[i];
		}

		public void Sort()
		{
		    Nodes.Sort();
		}

		public NodeChildren(HashSet<T> inlist)
		{
			Nodes = new List<T>(inlist);
			memberCount = Nodes.Count;
		}

		public NodeChildren(NodeChildren<T> inlist)
		{
			Nodes = new List<T>(inlist);
			memberCount = Nodes.Count;
		}

		/// <summary>
		/// Ensures there are no duplicate adds
		/// </summary>
		/// <param name="node"></param>
		public void Add(T node)
		{
			if (!Nodes.Contains(node))
			{
				Nodes.Add(node);
				memberCount++;
			}

		}

		public void AddRange(NodeChildren<T> nodes)
		{
		    Nodes.AddRange(nodes);
		}

		/// <summary>
		/// Ensures removal is smooth
		/// </summary>
		/// <param name="node"></param>
		public void RemoveChild(T node)
		{
			if (Nodes.Contains(node))
			{
				Nodes.Remove(node);
				memberCount--;
			}
		}


		public int IndexOf(T item)
		{
			return Nodes.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			if (!Nodes.Contains(item))
			{
				Nodes.Insert(index, item);
				memberCount++;
			}

		}

		public void RemoveAt(int index)
		{
			Nodes.RemoveAt(index);
		}

		public T this[int index]
		{
			get => Nodes[index];
		    set => Nodes[index] = value;
		}


		public void Clear()
		{
			Nodes.Clear();
		}

		public bool Contains(T item)
		{
			return Nodes.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Nodes.CopyTo(array, arrayIndex);
		}

		public int Count => Nodes.Count;

	    public bool IsReadOnly => false;

	    public bool Remove(T item)
		{
			return Nodes.Remove(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Nodes.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
