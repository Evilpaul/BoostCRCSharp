using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boost.Detail
{
	/// <summary>
	///  вспомогательный класс, для обращения чисел
	/// </summary>
	class Reflector<T> where T : new()
	{
		/// <summary>
		/// битность для конкретной реализации рефлектора
		/// </summary>
		public readonly int Bits;

		/// <summary>
		/// обращение бит в целом числе
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static T reflect(T x, int Bits)
		{
			dynamic reflection = new T();

			dynamic one = new T();	// 1
			one++;

			dynamic SourceX = x;

			for (int i = 0; i < Bits; ++i, SourceX >>= 1)
			{
				dynamic TestedBit = SourceX & one;

				if (TestedBit == one)
				{
					int index = Bits - 1 - i;

					dynamic TmpVal = one;

					TmpVal <<= index;

					reflection |= TmpVal;
				}
			}

			return (T)reflection;
		}

		public Reflector(int Bits)
		{
			this.Bits = Bits;
		}

		public T reflect(T x)
		{
			return reflect(x, Bits);
		}
	}
}
