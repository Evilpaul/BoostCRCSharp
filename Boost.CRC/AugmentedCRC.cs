using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

using Boost.Detail;

namespace Boost.CRC
{
    public class AugmentedCRC<T> where T: new()
    {
		/// <summary>
		/// Степень алгоритма, выраженная в битах.
		/// </summary>
		/// <remarks> Она всегда на единицу меньше длины полинома, но равна его степени.</remarks>
		public readonly int Bits;

		/// <summary>
		/// Собственно полином.
		/// </summary>
		/// <remarks>
		/// Это битовая величина, которая для удобства может быть представлена шестнадцатеричным числом. Старший бит при этом опускается.
		/// Например, если используется полином 10110, то он обозначается числом "06h". Важной особенностью данного параметра является то,
		/// что он всегда представляет собой необращенный полином, младшая часть этого параметра во время вычислений всегда является
		/// наименее значащими битами делителя вне зависимости от того, какой – "зеркальный" или прямой алгоритм моделируется.
		/// </remarks>
		public readonly T TruncPoly;

		/// <summary>
		/// рабочие маски
		/// </summary>
		private readonly MaskUint<T> masking_type;

		private readonly CRCtable<T> crc_table_type;

		/// <summary>
		/// определение того что тип является BigInteger.
		/// </summary>
		/// <remarks>для данного типа нужны отдельные заморочки</remarks>
		readonly bool bIsBigIntegerType = typeof(T) == typeof(BigInteger);

		/// <summary>
		/// пороговое значение обработанных байт для BigInteger
		/// </summary>
		const int BIByteProcessThresold = 10;

		public AugmentedCRC(int Bits, T TruncPoly)
		{
			this.Bits = Bits;
			this.TruncPoly = TruncPoly;

			masking_type = new MaskUint<T>(Bits);

			crc_table_type = new CRCtable<T>(Bits, TruncPoly, false);

			crc_table_type.InitTable();
		}

		public T Calculate(byte[] buffer, int offset, int size, T initial_remainder)
		{
			/// число обработанных байт в режиме работы с BigInteger
			int BIByteProcessCount = BIByteProcessThresold;

			dynamic rem = initial_remainder;

			for (int x = 0; x < size; x++)
			{
				byte p = buffer[offset + x];

				// Use the current top byte as the table index to the next
				// "partial product."  Shift out that top byte, shifting in
				// the next augmented-message byte.  Complete the division.

				dynamic TopByte=rem >> (Bits - Limits.CHAR_BIT);

				if (bIsBigIntegerType)
				{
					// это не обязательно для обычных целых типов. И просто кровь из носу требуется выполнять для BigInteger.

					// тут есть шанс словить эксепшен. защита от этого
					TopByte &= 0xFF;

					// регистр растёт в размере на 8 бит после обработки одного байта
					// нужен разумный компромисс между размером регистра и частотой сброса старших бит

					BIByteProcessCount--;

					if (BIByteProcessCount <= 0)
					{
						rem &= masking_type.SigBits;

						BIByteProcessCount = BIByteProcessThresold;
					}
				}

				byte byte_index = (byte)TopByte;
				rem <<= Limits.CHAR_BIT;
				rem |= p;
				rem ^= crc_table_type.Table[byte_index];
			}

			return (T)(rem & masking_type.SigBits);
		}
    }
}
