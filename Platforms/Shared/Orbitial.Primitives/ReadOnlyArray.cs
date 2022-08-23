using System;
using System.Collections;
using System.Collections.Generic;

namespace Orbital.Primitives
{
	public class ReadOnlyArray<T> : IEnumerable<T>
	{
		private T[] array;

		public void Copy(T[] dstArray)
		{
			Array.Copy(array, dstArray, dstArray.Length);
		}

		public ReadOnlyArray(int length, out T[] backingArray)
		{
			backingArray = new T[length];
			array = backingArray;
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
			return new Enumerator(array);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return array.GetEnumerator();
		}

		public struct Enumerator : IEnumerator<T>
		{
			private T[] array;
			private int index;

			public Enumerator(T[] array)
			{
				this.array = array;
				index = -1;
			}

			public T Current
			{
				get
				{
					return array[index];
				}
			}

			object IEnumerator.Current => throw new System.NotImplementedException();

			public void Dispose()
			{
				array = null;
			}

			public bool MoveNext()
			{
				++index;
				return index != array.Length;
			}

			public void Reset()
			{
				index = -1;
			}
		}
	}
}
