using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using MiscUtil;
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
			if (!DoReflect)
			{
				if (Bits > Limits.CHAR_BIT)
					rem = Operator<T>.RightShift(rem, Bits - Limits.CHAR_BIT);
				else
					rem = Operator<T>.LeftShift(rem, Limits.CHAR_BIT - Bits);
			}

			rem = Operator<T>.Xor(rem, Operator<byte, T>.Convert(x) );

			if (bIsBigIntegerType)
			{
				// следующая строчка важна для класса BigInteger, но не требуется для встроенных численных типов.
				// BigInteger может кидать исключение при преобразовании к byte. Мы должны дать гарантию приведения к byte без
				// швыряния исключения.
				rem = Operator<T>.And(rem, MaskUint<T>.Value0xFF);		// гарантия того что число не превышает значения байта
			}

			return Operator<T, byte>.Convert(rem);
		}

		// Shift out the remainder's highest byte
		public T shift(T rem)
		{
			if (DoReflect)
				return Operator<T>.RightShift(rem, Limits.CHAR_BIT);
			else
				return Operator<T>.LeftShift(rem, Limits.CHAR_BIT);
		}
	}
}
