using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.CRC;

namespace Boost.Detail
{
	/// <summary>
	/// вспомогательный класс для масок
	/// обобщённая форма
	/// </summary>
	/// <remarks>идея спёрта у boost</remarks>
	struct MaskUint<T> where T : new()
	{
		/// <summary>
		/// маска - один установленный бит в старшем разряде регистра текущей разрядности
		/// </summary>
		public readonly T HighBitMask;

		/// <summary>
		/// маска значащих бит в регистре текущей разрядности
		/// </summary>
		public readonly T SigBits;

		public const byte ByteHighBitMask = 1 << (Limits.CHAR_BIT - 1);

		public MaskUint(int Bits)
		{
			dynamic val1 = new T();

			val1 += 1;

			val1 <<= (Bits - 1);

			HighBitMask = (T)val1;

			dynamic val2 = new T();

			val2 += 1;

			// специальный прикол для защиты от сдвига разрядность регистра/сдвиг - 32/32 64/64
			val2 <<= (Bits / 2);
			val2 <<= (Bits - Bits / 2);

			val2--;

			SigBits = (T)val2;
		}
	}
}
