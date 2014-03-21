using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Numerics;

using Boost.CRC;


namespace CRCTest
{
	/// <summary>
	/// стандартное описание алгоритма
	/// </summary>
	class SimpleTestAlgorithm
	{
		public readonly string Name;
		public readonly int Width;
		public readonly BigInteger Poly;
		public readonly BigInteger Init;
		public readonly bool RefIn;
		public readonly bool RefOut;
		public readonly BigInteger XorOut;
		public readonly BigInteger Check;

		// данные для тестирования
		readonly static byte[] StandartCheckData = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };

		public SimpleTestAlgorithm(string Name, int Width, BigInteger Poly, BigInteger Init, bool RefIn, bool RefOut, BigInteger XorOut, BigInteger Check)
		{
			this.Name = Name;
			this.Width = Width;
			this.Poly = Poly;
			this.Init = Init;
			this.RefIn = RefIn;
			this.RefOut = RefOut;
			this.XorOut = XorOut;
			this.Check = Check;
		}

		public void Test()
		{
			{
				CRCbasic<BigInteger> crcbasic = new CRCbasic<BigInteger>(Width, Poly, Init, XorOut, RefIn, RefOut);
				CRCoptimal<BigInteger> crcopt = new CRCoptimal<BigInteger>(Width, Poly, Init, XorOut, RefIn, RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == Check);
			}
			
			if(Width<=64)
			{
				CRCbasic<ulong> crcbasic = new CRCbasic<ulong>(Width, (ulong)Poly, (ulong)Init, (ulong)XorOut, RefIn, RefOut);
				CRCoptimal<ulong> crcopt = new CRCoptimal<ulong>(Width, (ulong)Poly, (ulong)Init, (ulong)XorOut, RefIn, RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == Check);
			}

			if (Width <= 32)
			{
				CRCbasic<uint> crcbasic = new CRCbasic<uint>(Width, (uint)Poly, (uint)Init, (uint)XorOut, RefIn, RefOut);
				CRCoptimal<uint> crcopt = new CRCoptimal<uint>(Width, (uint)Poly, (uint)Init, (uint)XorOut, RefIn, RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == Check);
			}

			if (Width <= 16)
			{
				CRCbasic<ushort> crcbasic = new CRCbasic<ushort>(Width, (ushort)Poly, (ushort)Init, (ushort)XorOut, RefIn, RefOut);
				CRCoptimal<ushort> crcopt = new CRCoptimal<ushort>(Width, (ushort)Poly, (ushort)Init, (ushort)XorOut, RefIn, RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == Check);
			}

			if (Width <= 8)
			{
				CRCbasic<byte> crcbasic = new CRCbasic<byte>(Width, (byte)Poly, (byte)Init, (byte)XorOut, RefIn, RefOut);
				CRCoptimal<byte> crcopt = new CRCoptimal<byte>(Width, (byte)Poly, (byte)Init, (byte)XorOut, RefIn, RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == Check);
			}
		}
	}
}
