using System.Collections;
using System.Collections.Generic;

namespace Orbital.Primitives
{
	public class ReadOnlyArray<T> : IEnumerable<T>
	{
		private T[] array;

		public static ReadOnlyArray<T> Create(int length, out T[] backingArray)
		{
			backingArray = new T[length];
			return new ReadOnlyArray<T>(backingArray);
		}

		public ReadOnlyArray(T[] backingArray)
		{
			array = backingArray;
		}

		public T this[int i]
		{
			get { return array[i]; }
		}

		public int Length
		{
			get { return array.Length; }
		}

		public IEnumerator<T> GetEnumerator()
		{
			return (IEnumerator<T>)array.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return array.GetEnumerator();
		}
	}
}
