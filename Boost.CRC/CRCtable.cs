using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;

namespace Boost.CRC
{
	class CRCtable
	{
		/// <summary>
		/// таблица для ускорения расчётов
		/// </summary>
		private ulong[] Table_ = new ulong[1 << Limits.CHAR_BIT];

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>ВНИМАНИЕ!! Данная таблица содержит лишние биты в каждом значении. Вычислениям это не мешает. Однако при портировании расчитанной
		/// таблицы стоит иметь ввиду что в каждой записи из таблицы нужно дополнительно ещё обрезать старшие биты до разрядности регистра
		/// </remarks>
		public ulong[] Table
		{
			get
			{
				return Table_;
			}
		}


		public readonly int Bits;
		public readonly ulong TruncPoly;
		public readonly bool Reflect;


		private readonly MaskUint masking_type;

		public CRCtable(int Bits, ulong TruncPoly, bool Reflect)
		{
			this.Bits = Bits;
			this.TruncPoly = TruncPoly;
			this.Reflect = Reflect;

			masking_type = new MaskUint(Bits);
		}


		/// <summary>
		/// инициализирует таблицу для быстрого расчёта
		/// </summary>
		public void InitTable()
		{
			// В целом рассчёт таблицы проводится правильно.
			// Только из-за использования регистра большей разрядности надо брать именно Bits младших бит

			CRChelper charRefl = new CRChelper(Limits.CHAR_BIT, Reflect);
			CRChelper BitsRefl = new CRChelper(Bits, Reflect);

			// factor-out constants to avoid recalculation
			ulong fast_hi_bit = masking_type.HighBitMask;
			const byte byte_hi_bit = MaskUint.ByteHighBitMask;

			// loop over every possible dividend value
			byte dividend = 0;

			do
			{
				ulong remainder = 0;

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

				Table_[charRefl.reflect(dividend)] = BitsRefl.reflect(remainder);
				++dividend;
			}
			while (dividend != 0);
		}
	}
}
