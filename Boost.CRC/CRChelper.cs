using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Boost.CRC;

namespace Boost.Detail
{
	/// <summary>
	/// вспомогательный класс
	/// </summary>
	class CRChelper<T> where T : new()
	{
		public readonly int Bits;
		public readonly bool DoReflect;

		/// <summary>
		/// определение того что тип является BigInteger.
		/// </summary>
		/// <remarks>для данного типа нужны отдельные заморочки</remarks>
		readonly bool bIsBigIntegerType = typeof(T)==typeof(BigInteger);

		public CRChelper(int Bits, bool DoReflect)
		{
			this.Bits = Bits;
			this.DoReflect = DoReflect;
		}

		public T reflect(T x)
		{
			if (DoReflect)
				return Reflector<T>.reflect(x, Bits);
			else
				return x;
		}

		// Compare a byte to the remainder's highest byte
		public byte index(T rem, byte x)
		{
			dynamic tval = rem;

			if (!DoReflect)
			{
				if (Bits > Limits.CHAR_BIT)
					tval = tval >> (Bits - Limits.CHAR_BIT);
				else
					tval = tval << (Limits.CHAR_BIT - Bits);
			}

			tval ^= x;

			if (bIsBigIntegerType)
			{
				// следующая строчка важна для класса BigInteger, но не требуется для встроенных численных типов.
				// BigInteger может кидать исключение при преобразовании к byte. Мы должны дать гарантию приведения к byte без
				// швыряния исключения.
				tval &= 0xFF;		// гарантия того что число не превышает значения байта
			}

			return (byte)tval;
		}

		// Shift out the remainder's highest byte
		public T shift(T rem)
		{
			dynamic tmp = rem;

			if (DoReflect)
			{
				return (T)(tmp >> Limits.CHAR_BIT);
			}
			else
			{
				return (T)(tmp << Limits.CHAR_BIT);
			}
		}
	}
}
