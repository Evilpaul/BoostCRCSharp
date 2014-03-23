using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MiscUtil;
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

		/// <summary>
		/// старший бит в байте
		/// </summary>
		public const byte ByteHighBitMask = 1 << (Limits.CHAR_BIT - 1);

		/// <summary>
		/// значение 1-ы в генерике
		/// </summary>
		public static readonly T One = Operator<int, T>.Convert(1);

		/// <summary>
		/// значение 0xFF в генерике
		/// </summary>
		public static readonly T Value0xFF = Operator<int, T>.Convert(0xFF);

		public MaskUint(int Bits)
		{
			HighBitMask = Operator<T>.LeftShift(One, Bits - 1);

			// специальный прикол для защиты от сдвига разрядность регистра/сдвиг - 32/32 64/64
			int PartShift=Bits / 2;

			SigBits =Operator<T>.Not(Operator<T>.Zero);

			SigBits = Operator<T>.LeftShift(SigBits, PartShift);
			SigBits = Operator<T>.LeftShift(SigBits, Bits - PartShift);

			SigBits = Operator<T>.Not(SigBits);
		}
	}
}
