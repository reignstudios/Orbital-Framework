using System.Collections;
using System.Collections.Generic;

namespace Orbital.Primitives
{
	public class ReadOnlyHashSet<T> : IEnumerable<T>
	{
		private HashSet<T> hashset;

		public static ReadOnlyHashSet<T> Create(out HashSet<T> backingHashSet)
		{
			backingHashSet = new HashSet<T>();
			return new ReadOnlyHashSet<T>(backingHashSet);
		}

		public ReadOnlyHashSet(HashSet<T> backingHashSet)
		{
			hashset = backingHashSet;
		}

		public int Count
		{
			get { return hashset.Count; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return hashset.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return hashset.GetEnumerator();
		}
	}
}
