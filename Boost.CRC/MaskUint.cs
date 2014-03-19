using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.CRC;

namespace Boost.Detail
{
	/// <summary>
	/// вспомогательный класс для масок
	/// </summary>
	/// <remarks>идея спёрта у boost</remarks>
	struct MaskUint
	{
		/// <summary>
		/// маска - один установленный бит в старшем разряде регистра текущей разрядности
		/// </summary>
		public readonly ulong HighBitMask;

		/// <summary>
		/// маска значащих бит в регистре текущей разрядности
		/// </summary>
		public readonly ulong SigBits;

		public const byte ByteHighBitMask = 1 << (Limits.CHAR_BIT - 1);

		public MaskUint(int Bits)
		{
			HighBitMask = 1UL << (Bits - 1);

			SigBits = ~(0UL);

			if (Bits == 64)
			{
				// большой привет микрософту
				SigBits <<= 63;
				SigBits <<= 1;
			}
			else
				SigBits <<= Bits;

			SigBits = ~SigBits;
		}
	}
}
