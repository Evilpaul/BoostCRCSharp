using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			if (DoReflect)
				return (byte)(tval ^ x);
			else
			{
				if (Bits > Limits.CHAR_BIT)
					tval = tval >> (Bits - Limits.CHAR_BIT);
				else
					tval = tval << (Limits.CHAR_BIT - Bits);

				return (byte)(tval ^ x);
			}
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
