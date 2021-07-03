using System.Collections;
using System.Collections.Generic;

namespace Orbital.Primitives
{
	public class ReadOnlyList<T> : IEnumerable<T>
	{
		private List<T> list;

		public ReadOnlyList(out List<T> backingList)
		{
			backingList = new List<T>();
			list = backingList;
		}

		public ReadOnlyList(List<T> backingList)
		{
			list = backingList;
		}

		public T this[int i]
		{
			get { return list[i]; }
		}

		public int Count
		{
			get { return list.Count; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}
	}
}
