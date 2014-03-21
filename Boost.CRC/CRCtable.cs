using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;

namespace Boost.CRC
{
	class CRCtable<T> where T: new()
	{
		/// <summary>
		/// таблица для ускорения расчётов
		/// </summary>
		private T[] Table_ = new T[1 << Limits.CHAR_BIT];

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>ВНИМАНИЕ!! Данная таблица содержит лишние биты в каждом значении. Вычислениям это не мешает. Однако при портировании расчитанной
		/// таблицы стоит иметь ввиду что в каждой записи из таблицы нужно дополнительно ещё обрезать старшие биты до разрядности регистра
		/// </remarks>
		public T[] Table
		{
			get
			{
				return Table_;
			}
		}


		public readonly int Bits;
		public readonly T TruncPoly;
		public readonly bool Reflect;


		private readonly MaskUint<T> masking_type;

		public CRCtable(int Bits, T TruncPoly, bool Reflect)
		{
			this.Bits = Bits;
			this.TruncPoly = TruncPoly;
			this.Reflect = Reflect;

			masking_type = new MaskUint<T>(Bits);
		}


		/// <summary>
		/// инициализирует таблицу для быстрого расчёта
		/// </summary>
		public void InitTable()
		{
			CRChelper<byte> charRefl = new CRChelper<byte>(Limits.CHAR_BIT, Reflect);
			CRChelper<T> BitsRefl = new CRChelper<T>(Bits, Reflect);

			// factor-out constants to avoid recalculation
			T fast_hi_bit = masking_type.HighBitMask;
			const byte byte_hi_bit = MaskUint<T>.ByteHighBitMask;

			// loop over every possible dividend value
			byte dividend = 0;

			do
			{
				dynamic remainder = new T();

				// go through all the dividend's bits
				for (byte mask = byte_hi_bit; mask > 0; mask >>= 1)
				{
					// check if divisor fits
					if ((dividend & mask) != 0)
						remainder ^= fast_hi_bit;

					// do polynominal division
					if ((remainder & fast_hi_bit) != 0)
					{
						remainder <<= 1;
						remainder ^= TruncPoly;
					}
					else
						remainder <<= 1;
				}

				dynamic tmp = BitsRefl.reflect((T)remainder);

				// следующее выражение необязательно. Просто можно обнулить старшие(неиспользуемые) биты в регистре
				//tmp &= masking_type.SigBits;

				Table_[charRefl.reflect(dividend)] = (T)tmp;
				++dividend;
			}
			while (dividend != 0);
		}
	}
}
