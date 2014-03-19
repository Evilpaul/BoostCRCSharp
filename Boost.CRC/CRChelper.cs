using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.CRC;

namespace Boost.Detail
{
	class CRChelper
	{
		public readonly int Bits;
		public readonly bool DoReflect;

		public CRChelper(int Bits, bool DoReflect)
		{
			this.Bits = Bits;
			this.DoReflect = DoReflect;
		}

		public ulong reflect(ulong x)
		{
			if (DoReflect)
				return Reflector.reflect(x, Bits);
			else
				return x;
		}

		// Compare a byte to the remainder's highest byte
		public byte index(ulong rem, byte x)
		{
			if (DoReflect)
				return (byte)(x ^ rem);
			else
			{
				ulong tval;

				if (Bits > Limits.CHAR_BIT)
					tval = rem >> (Bits - Limits.CHAR_BIT);
				else
					tval = rem << (Limits.CHAR_BIT - Bits);

				return (byte)(x ^ tval);
			}
		}

		// Shift out the remainder's highest byte
		public ulong shift(ulong rem)
		{
			if (DoReflect)
				return rem >> Limits.CHAR_BIT;
			else
				return rem << Limits.CHAR_BIT;
		}
	}
}
