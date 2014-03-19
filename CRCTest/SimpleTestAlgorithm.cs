using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRCTest
{
	/// <summary>
	/// стандартное описание алгоритма
	/// </summary>
	class SimpleTestAlgorithm
	{
		public readonly string Name;
		public readonly int Width;
		public readonly ulong Poly;
		public readonly ulong Init;
		public readonly bool RefIn;
		public readonly bool RefOut;
		public readonly ulong XorOut;
		public readonly ulong Check;

		public SimpleTestAlgorithm(string Name, int Width, ulong Poly, ulong Init, bool RefIn, bool RefOut, ulong XorOut, ulong Check)
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
	}
}
