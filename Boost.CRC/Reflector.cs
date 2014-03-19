using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boost.Detail
{
	/// <summary>
	///  вспомогательный класс, для обращения чисел
	/// </summary>
	class Reflector
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
		public static ulong reflect(ulong x, int Bits)
		{
			ulong reflection = 0;
			const ulong one = 1;

			for (int i = 0; i < Bits; ++i, x >>= 1)
				if ((x & one) != 0)
					reflection |= (ulong)(one << (Bits - 1 - i));

			return reflection;
		}

		public Reflector(int Bits)
		{
			this.Bits = Bits;
		}

		public ulong reflect(ulong x)
		{
			return reflect(x, Bits);
		}
	}
}
