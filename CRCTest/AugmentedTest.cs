using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Diagnostics;
using System.IO;

using Boost.CRC;

namespace CRCTest
{
	/// <summary>
	/// класс проведения теста для Augmented CRC-функций
	/// </summary>
	/// <typeparam name="T"></typeparam>
	class AugmentedTest<T> where T : new()
	{
		static T TfromHexString(string strval)
		{
			Type t = typeof(T);

			MethodInfo meth = t.GetMethod("Parse", new Type[] { typeof(string), typeof(NumberStyles) });

			object tt1 = meth.Invoke(null, new object[] { strval, NumberStyles.AllowHexSpecifier });

			return (T)tt1;
		}

		static T TfromString(string strval)
		{
			Type t = typeof(T);

			MethodInfo meth = t.GetMethod("Parse", new Type[] { typeof(string) });

			object tt1 = meth.Invoke(null, new object[] { strval });

			return (T)tt1;
		}

		static void InternalTest(byte[] ran_data)
		{
			// наш рабочий полином
			string strAlgoPolynom = "04C11DB7";
			T AlgoPolynom = TfromHexString(strAlgoPolynom);

			int data_length = ran_data.Length - 4;

			// When creating a CRC for an augmented message, use
			// zeros in the appended CRC spot for the first run.
			ran_data[data_length + 0] = 0;
			ran_data[data_length + 1] = 0;
			ran_data[data_length + 2] = 0;
			ran_data[data_length + 3] = 0;

			AugmentedCRC<T> aug = new AugmentedCRC<T>(32, AlgoPolynom);

			// Compute the CRC with augmented-CRC computing function
			T Tran_crc = aug.Calculate(ran_data, 0, ran_data.Length, new T());

			uint ran_crc = uint.Parse(Tran_crc.ToString());

			// With the appended CRC set, running the checksum again should get zero.
			// NOTE: CRC algorithm assumes numbers are in big-endian format

			ran_data[data_length + 3] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 2] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 1] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 0] = (byte)ran_crc;

			T Tran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length, new T());

			uint ran_crc_check = uint.Parse(Tran_crc_check.ToString());

			Debug.Assert(ran_crc_check == 0);

			// Compare that result with other CRC computing functions
			// and classes, which don't accept augmented messages.

			CRCoptimal<T> fast_tester = new CRCoptimal<T>(32, AlgoPolynom, new T(), new T());
			CRCbasic<T> slow_tester = new CRCbasic<T>(32, AlgoPolynom, new T(), new T());

			fast_tester.ProcessBytes(ran_data, data_length);
			slow_tester.ProcessBytes(ran_data, data_length);

			T Tfast_testerCheckSum = fast_tester.CheckSum;
			T Tslow_testerCheckSum = slow_tester.CheckSum;

			uint fast_testerCheckSum = uint.Parse(Tfast_testerCheckSum.ToString());

			uint slow_testerCheckSum = uint.Parse(Tslow_testerCheckSum.ToString());

			Debug.Assert(fast_testerCheckSum == slow_testerCheckSum);

			ran_crc = 0;
			ran_crc |= ran_data[data_length + 0];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 1];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 2];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 3];

			Debug.Assert(fast_testerCheckSum == ran_crc);

			// Do a single-bit error test

			ran_data[ran_data[0] % ran_data.Length] ^= (byte)(1 << (ran_data[1] % 8));

			Tran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length, new T());

			ran_crc_check = uint.Parse(Tran_crc_check.ToString());

			Debug.Assert(ran_crc_check != 0);

			// Run a version of these tests with a nonzero initial remainder.

			int ind = ran_data[2] % data_length;
			uint init_rem = 0;

			init_rem |= ran_data[ind + 0];
			init_rem <<= 8;
			init_rem |= ran_data[ind + 1];
			init_rem <<= 8;
			init_rem |= ran_data[ind + 2];
			init_rem <<= 8;
			init_rem |= ran_data[ind + 3];

			// обнулить CRC
			ran_data[data_length + 0] = 0;
			ran_data[data_length + 1] = 0;
			ran_data[data_length + 2] = 0;
			ran_data[data_length + 3] = 0;

			Tran_crc = aug.Calculate(ran_data, 0, ran_data.Length,
				TfromString(init_rem.ToString())
				);

			ran_crc = uint.Parse(Tran_crc.ToString());

			ran_data[data_length + 3] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 2] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 1] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 0] = (byte)ran_crc;

			// Have some fun by processing data in two steps.
			int mid_index = ran_data.Length / 2;

			Tran_crc_check = aug.Calculate(ran_data, 0, mid_index,
				TfromString(init_rem.ToString())
				);

			ran_crc_check = uint.Parse(Tran_crc_check.ToString());

			Tran_crc_check = aug.Calculate(ran_data, mid_index, ran_data.Length - mid_index,
				TfromString(ran_crc_check.ToString())
				);

			ran_crc_check = uint.Parse(Tran_crc_check.ToString());

			Debug.Assert(ran_crc_check == 0);

			// This substep translates an augmented-CRC initial
			// remainder to an unaugmented-CRC initial remainder.

			byte[] zero = new byte[4] { 0, 0, 0, 0 };

			T Tnew_init_rem = aug.Calculate(zero, 0, zero.Length,
				TfromString(init_rem.ToString())
				);

			uint new_init_rem = uint.Parse(Tnew_init_rem.ToString());

			CRCbasic<T> slow_tester2 = new CRCbasic<T>(32, AlgoPolynom,
				TfromString(new_init_rem.ToString()),
				new T());

			slow_tester2.ProcessBytes(ran_data, data_length);

			ran_crc = 0;
			ran_crc |= ran_data[data_length + 0];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 1];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 2];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 3];

			T Tslow_tester2CheckSum = slow_tester2.CheckSum;

			uint slow_tester2CheckSum = uint.Parse(Tslow_tester2CheckSum.ToString());

			Debug.Assert(slow_tester2CheckSum == ran_crc);

			// Redo single-bit error test

			ran_data[ran_data[3] % ran_data.Length] ^= (byte)(1 << (ran_data[4] % 8));

			Tran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length,
				TfromString(init_rem.ToString())
				);

			ran_crc_check = uint.Parse(Tran_crc_check.ToString());

			Debug.Assert(ran_crc_check != 0);
		}

		/// <summary>
		/// Create a random block of data, with space for a CRC.
		/// </summary>
		static byte[] NewArray
		{
			get
			{
				byte[] ran_data = new byte[257 * 4];

				Random random = new Random();

				random.NextBytes(ran_data);

				return ran_data;
			}
		}

		static void SaveArray(byte []ran_data, string Filename)
		{
			using (FileStream fstrm = new FileStream(Filename, FileMode.Create))
			{
				using(BinaryWriter bwr=new BinaryWriter(fstrm))
				{
					bwr.Write(ran_data);
				}
			}
		}

		static byte[] LoadArray(string Filename)
		{
			using (FileStream fstrm = new FileStream(Filename, FileMode.Open))
			{
				using (BinaryReader brd = new BinaryReader(fstrm))
				{
					return brd.ReadBytes(1000000);
				}
			}
		}

		/// <summary>
		/// Run tests on using CRCs on augmented messages
		/// </summary>
		public static void DoTest()
		{
			// Create a random block of data, with space for a CRC.
			byte[] ran_data;

			ran_data = NewArray;
			//SaveArray(ran_data, "1.bin");
			//ran_data = LoadArray("1.bin");

			InternalTest(ran_data);
		}
	}
}
