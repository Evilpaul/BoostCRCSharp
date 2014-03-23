using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MiscUtil;

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
			T reflection = Operator<T>.Zero;

			for (int i = 0; i < Bits; ++i, x = Operator <T>.RightShift(x, 1) )
			{
				T TestedBit = Operator<T>.And(x, MaskUint<T>.One);

				if (Operator<T>.Equal(TestedBit, MaskUint<T>.One))
				{
					int index = Bits - 1 - i;

					T TmpVal = Operator<T>.LeftShift(MaskUint<T>.One, index);

					reflection = Operator<T>.Or(reflection, TmpVal);
				}
			}

			return reflection;
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
