using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;

namespace Boost.CRC
{
    public class AugmentedCRC
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
		public readonly ulong TruncPoly;

		/// <summary>
		/// рабочие маски
		/// </summary>
		private readonly MaskUint masking_type;

		private readonly CRCtable crc_table_type;

		public AugmentedCRC(int Bits, ulong TruncPoly)
		{
			this.Bits = Bits;
			this.TruncPoly = TruncPoly;

			masking_type = new MaskUint(Bits);

			crc_table_type = new CRCtable(Bits, TruncPoly, false);

			crc_table_type.InitTable();
		}

		public ulong Calculate(byte[] buffer, int offset, int size, ulong initial_remainder = 0)
		{
			ulong rem = initial_remainder;

			for (int x = 0; x < size; x++)
			{
				byte p = buffer[offset + x];

				// Use the current top byte as the table index to the next
				// "partial product."  Shift out that top byte, shifting in
				// the next augmented-message byte.  Complete the division.
				byte byte_index = (byte)(rem >> (Bits - Limits.CHAR_BIT));
				rem <<= Limits.CHAR_BIT;
				rem |= p;
				rem ^= crc_table_type.Table[byte_index];
			}

			return rem & masking_type.SigBits;
		}
    }
}
