﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Boost.CRC;

namespace CRCTest
{
	class Program
	{
		// Run tests on using CRCs on augmented messages
		private static void AugmentedTests()
		{
			// Create a random block of data, with space for a CRC.
			byte[] ran_data = new byte[257 * 4];
			int data_length = ran_data.Length - 4;

			Random random = new Random();

			random.NextBytes(ran_data);

			// When creating a CRC for an augmented message, use
			// zeros in the appended CRC spot for the first run.
			ran_data[data_length + 0] = 0;
			ran_data[data_length + 1] = 0;
			ran_data[data_length + 2] = 0;
			ran_data[data_length + 3] = 0;

			AugmentedCRC aug = new AugmentedCRC(32, 0x04C11DB7);

			// Compute the CRC with augmented-CRC computing function
			ulong ran_crc = aug.Calculate(ran_data, 0, ran_data.Length);

			// With the appended CRC set, running the checksum again should get zero.
			// NOTE: CRC algorithm assumes numbers are in big-endian format

			ran_data[data_length + 3] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 2] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 1] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 0] = (byte)ran_crc;

			ulong ran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length);

			Debug.Assert(0 == ran_crc_check);

			// Compare that result with other CRC computing functions
			// and classes, which don't accept augmented messages.

			CRCoptimal fast_tester = new CRCoptimal(32, 0x04C11DB7);
			CRCbasic slow_tester = new CRCbasic(32, 0x04C11DB7);

			fast_tester.ProcessBytes(ran_data, data_length);
			slow_tester.ProcessBytes(ran_data, data_length);
			Debug.Assert(fast_tester.CheckSum == slow_tester.CheckSum);

			ran_crc = 0;
			ran_crc |= ran_data[data_length + 0];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 1];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 2];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 3];

			Debug.Assert(fast_tester.CheckSum == ran_crc);

			// Do a single-bit error test

			ran_data[ran_data[0] % ran_data.Length] ^= (byte)(1 << (ran_data[1] % 8));

			ran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length);

			Debug.Assert(ran_crc_check != 0);

			// Run a version of these tests with a nonzero initial remainder.

			int ind = ran_data[2] % data_length;
			ulong init_rem = 0;
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

			ran_crc = aug.Calculate(ran_data, 0, ran_data.Length, init_rem);
			ran_data[data_length + 3] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 2] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 1] = (byte)ran_crc;
			ran_crc >>= 8;
			ran_data[data_length + 0] = (byte)ran_crc;

			// Have some fun by processing data in two steps.
			int mid_index = ran_data.Length / 2;

			ran_crc_check = aug.Calculate(ran_data, 0, mid_index, init_rem);
			ran_crc_check = aug.Calculate(ran_data, mid_index, ran_data.Length - mid_index, ran_crc_check);

			Debug.Assert(ran_crc_check == 0);

			// This substep translates an augmented-CRC initial
			// remainder to an unaugmented-CRC initial remainder.

			byte[] zero = new byte[4] { 0, 0, 0, 0 };

			ulong new_init_rem = aug.Calculate(zero, 0, zero.Length, init_rem);

			CRCbasic slow_tester2 = new CRCbasic(32, 0x04C11DB7, new_init_rem);

			slow_tester2.ProcessBytes(ran_data, data_length);

			ran_crc = 0;
			ran_crc |= ran_data[data_length + 0];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 1];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 2];
			ran_crc <<= 8;
			ran_crc |= ran_data[data_length + 3];

			Debug.Assert(slow_tester2.CheckSum == ran_crc);

			// Redo single-bit error test

			ran_data[ran_data[3] % ran_data.Length] ^= (byte)(1 << (ran_data[4] % 8));

			ran_crc_check = aug.Calculate(ran_data, 0, ran_data.Length, init_rem);

			Debug.Assert(ran_crc_check != 0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>see http://regregex.bbcmicro.net/crc-catalogue.htm</remarks>
		private static void BasicTests()
		{
			byte[] StandartCheckData = new byte[] { 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 };

			SimpleTestAlgorithm[] alglist = new SimpleTestAlgorithm[]
			{
				new SimpleTestAlgorithm("какой-то CRC-8 алгоритм", 8, 0x31, 0xFF, false, false, 0x00, 0xF7),

				/*
				Name   : "CRC-3/ROHC"
				Width  : 3
				Poly   : 3
				Init   : 7
				RefIn  : True
				RefOut : True
				XorOut : 0
				Check  : 6 
				*/
				new SimpleTestAlgorithm("CRC-3/ROHC", 3, 0x3, 0x7, true, true, 0x0, 0x6),

				/*
				Name   : "CRC-4/ITU"
				Width  : 4
				Poly   : 3
				Init   : 0
				RefIn  : True
				RefOut : True
				XorOut : 0
				Check  : 7
				*/
				new SimpleTestAlgorithm("CRC-4/ITU", 4, 0x3, 0x0, true, true, 0x0, 0x7),

				/* 
				Name   : "CRC-5/EPC"
				Width  : 5
				Poly   : 09
				Init   : 09
				RefIn  : False
				RefOut : False
				XorOut : 00
				Check  : 00
				*/
				new SimpleTestAlgorithm("CRC-5/EPC", 5, 0x09, 0x09, false, false, 0x00, 0x00),

				/*
				Name   : "CRC-5/ITU"
				Width  : 5
				Poly   : 15
				Init   : 0
				RefIn  : True
				RefOut : True
				XorOut : 0
				Check  : 07
				*/
				new SimpleTestAlgorithm("CRC-5/ITU", 5, 0x15, 0x0, true, true, 0x0, 0x07),

				/*
				Name   : "CRC-5/USB"
				Width  : 5
				Poly   : 05
				Init   : 1F
				RefIn  : True
				RefOut : True
				XorOut : 1F
				Check  : 19
				*/
				new SimpleTestAlgorithm("CRC-5/USB", 5, 0x05, 0x1F, true, true, 0x1F, 0x19),

				/*
				Name   : "CRC-6/DARC"
				Width  : 6
				Poly   : 19
				Init   : 00
				RefIn  : True
				RefOut : False
				XorOut : 00
				Check  : 19
				*/
				new SimpleTestAlgorithm("CRC-6/DARC", 6, 0x19, 0x00, true, false, 0x00, 0x19),

				/*
				Name   : "CRC-6/ITU"
				Width  : 6
				Poly   : 03
				Init   : 00
				RefIn  : True
				RefOut : True
				XorOut : 00
				Check  : 06
				*/
				new SimpleTestAlgorithm("CRC-6/ITU", 6, 0x03, 0x00, true, true, 0x00, 0x06),

				/*
				Name   : "CRC-7"
				Width  : 7
				Poly   : 09
				Init   : 00
				RefIn  : False
				RefOut : False
				XorOut : 00
				Check  : 75
				*/
				new SimpleTestAlgorithm("CRC-7", 7, 0x09, 0x00, false, false, 0x00, 0x75),

				/*
				Name   : "CRC-7/ROHC"
				Width  : 7
				Poly   : 4F
				Init   : 7F
				RefIn  : True
				RefOut : True
				XorOut : 0
				Check  : 53
				*/
				new SimpleTestAlgorithm("CRC-7/ROHC", 7, 0x4F, 0x7F, true, true, 0x0, 0x53),

				/*
				Name   : "CRC-8"
				Width  : 8
				Poly   : 07
				Init   : 00
				RefIn  : False
				RefOut : False
				XorOut : 00
				Check  : F4
				*/
				new SimpleTestAlgorithm("CRC-8", 8, 0x07, 0x00, false, false, 0x00, 0xF4),

				/*
				Name   : "CRC-8/DARC"
				Width  : 8
				Poly   : 39
				Init   : 00
				RefIn  : True
				RefOut : True
				XorOut : 00
				Check  : 15
				*/
				new SimpleTestAlgorithm("CRC-8/DARC", 8, 0x39, 0x00, true, true, 0x00, 0x15),

				/*
				Name   : "CRC-8/I-CODE"
				Width  : 8
				Poly   : 1D
				Init   : FD
				RefIn  : False
				RefOut : False
				XorOut : 00
				Check  : 7E
				*/
				new SimpleTestAlgorithm("CRC-8/I-CODE", 8, 0x1D, 0xFD, false, false, 0x00, 0x7E),

				/*
				Name   : "CRC-8/ITU"
				Width  : 8
				Poly   : 07
				Init   : 00
				RefIn  : False
				RefOut : False
				XorOut : 55
				Check  : A1
				*/
				new SimpleTestAlgorithm("CRC-8/ITU", 8, 0x07, 0x00, false, false, 0x55, 0xA1),

				/*
				Name   : "CRC-8/MAXIM"
				Alias  : "DOW-CRC"
				Width  : 8
				Poly   : 31
				Init   : 00
				RefIn  : True
				RefOut : True
				XorOut : 00
				Check  : A1
				*/
				new SimpleTestAlgorithm("CRC-8/MAXIM", 8, 0x31, 0x00, true, true, 0x00, 0xA1),

				/*
				Name   : "CRC-8/ROHC"
				Width  : 8
				Poly   : 07
				Init   : FF
				RefIn  : True
				RefOut : True
				XorOut : 0
				Check  : D0
				*/
				new SimpleTestAlgorithm("CRC-8/ROHC", 8, 0x07, 0xFF, true, true, 0x00, 0xD0),

				/*
				Name   : "CRC-8/WCDMA"
				Width  : 8
				Poly   : 9B
				Init   : 00
				RefIn  : True
				RefOut : True
				XorOut : 00
				Check  : 25
				*/
				new SimpleTestAlgorithm("CRC-8/WCDMA", 8, 0x9B, 0x00, true, true, 0x00, 0x25),

				/*
				Name   : "CRC-10"
				Width  : 10
				Poly   : 233
				Init   : 000
				RefIn  : False
				RefOut : False
				XorOut : 000
				Check  : 199
				*/
				new SimpleTestAlgorithm("CRC-10", 10, 0x233, 0x000, false, false, 0x000, 0x199),

				/*
				Name   : "CRC-11"
				Width  : 11
				Poly   : 385
				Init   : 01A
				RefIn  : False
				RefOut : False
				XorOut : 000
				Check  : 5A3
				*/
				new SimpleTestAlgorithm("CRC-11", 11, 0x385, 0x01A, false, false, 0x000, 0x5A3),

				/*
				Name   : "CRC-12/3GPP"
				Width  : 12
				Poly   : 80F
				Init   : 000
				RefIn  : False
				RefOut : True
				XorOut : 000
				Check  : DAF
				*/
				new SimpleTestAlgorithm("CRC-12/3GPP", 12, 0x80F, 0x000, false, true, 0x000, 0xDAF),

				/*
				Name   : "CRC-12/DECT"
				Alias  : "X-CRC-12"
				Width  : 12
				Poly   : 80F
				Init   : 000
				RefIn  : False
				RefOut : False
				XorOut : 000
				Check  : F5B
				*/
				new SimpleTestAlgorithm("CRC-12/DECT", 12, 0x80F, 0x000, false, false, 0x000, 0xF5B),

				/*
				Name   : "CRC-14/DARC"
				Width  : 14
				Poly   : 0805
				Init   : 0000
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : 082D
				*/
				new SimpleTestAlgorithm("CRC-14/DARC", 14, 0x0805, 0x0000, true, true, 0x0000, 0x082D),

				/*
				Name   : "CRC-15"
				Width  : 15
				Poly   : 4599
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 059E
				*/
				new SimpleTestAlgorithm("CRC-15", 15, 0x4599, 0x0000, false, false, 0x0000, 0x059E),

				/*
				Name   : "ARC"
				Alias  : "CRC-16"
				Alias  : "CRC-IBM"
				Alias  : "CRC-16/ARC"
				Alias  : "CRC-16/LHA"
				Width  : 16
				Poly   : 8005
				Init   : 0000
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : BB3D
				*/
				new SimpleTestAlgorithm("ARC", 16, 0x8005, 0x0000, true, true, 0x0000, 0xBB3D),

				/*
				Name   : "CRC-16/AUG-CCITT"
				Alias  : "CRC-16/SPI-FUJITSU"
				Width  : 16
				Poly   : 1021
				Init   : 1D0F
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : E5CC
				*/
				new SimpleTestAlgorithm("CRC-16/AUG-CCITT", 16, 0x1021, 0x1D0F, false, false, 0x0000, 0xE5CC),

				/*
				Name   : "CRC-16/BUYPASS"
				Alias  : "CRC-16/VERIFONE"
				Width  : 16
				Poly   : 8005
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : FEE8
				*/
				new SimpleTestAlgorithm("CRC-16/BUYPASS", 16, 0x8005, 0x0000, false, false, 0x0000, 0xFEE8),

				/*
				Name   : "CRC-16/CCITT-FALSE"
				Width  : 16
				Poly   : 1021
				Init   : FFFF
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 29B1
				*/
				new SimpleTestAlgorithm("CRC-16/CCITT-FALSE", 16, 0x1021, 0xFFFF, false, false, 0x0000, 0x29B1),
				/*
				Name   : "CRC-16/DDS-110"
				Width  : 16
				Poly   : 8005
				Init   : 800D
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 9ECF
				XCheck : CFE9
				*/
				new SimpleTestAlgorithm("CRC-16/DDS-110", 16, 0x8005, 0x800D, false, false, 0x0000, 0x9ECF),

				/*
				Name   : "CRC-16/DECT-R"
				Alias  : "R-CRC-16"
				Width  : 16
				Poly   : 0589
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0001
				Check  : 007E
				*/
				new SimpleTestAlgorithm("CRC-16/DECT-R", 16, 0x0589, 0x0000, false, false, 0x0001, 0x007E),

				/*
				Name   : "CRC-16/DECT-X"
				Alias  : "X-CRC-16"
				Width  : 16
				Poly   : 0589
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 007F
				*/
				new SimpleTestAlgorithm("CRC-16/DECT-X", 16, 0x0589, 0x0000, false, false, 0x0000, 0x007F),

				/*
				Name   : "CRC-16/DNP"
				Width  : 16
				Poly   : 3D65
				Init   : 0000
				RefIn  : True
				RefOut : True
				XorOut : FFFF
				Check  : EA82
				XCheck : 82EA
				*/
				new SimpleTestAlgorithm("CRC-16/DNP", 16, 0x3D65, 0x0000, true, true, 0xFFFF, 0xEA82),

				/*
				Name   : "CRC-16/EN-13757"
				Width  : 16
				Poly   : 3D65
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : FFFF
				Check  : C2B7
				*/
				new SimpleTestAlgorithm("CRC-16/EN-13757", 16, 0x3D65, 0x0000, false, false, 0xFFFF, 0xC2B7),

				/*
				Name   : "CRC-16/GENIBUS"
				Alias  : "CRC-16/EPC"
				Alias  : "CRC-16/I-CODE"
				Alias  : "CRC-16/DARC"
				Width  : 16
				Poly   : 1021
				Init   : FFFF
				RefIn  : False
				RefOut : False
				XorOut : FFFF
				Check  : D64E
				*/
				new SimpleTestAlgorithm("CRC-16/GENIBUS", 16, 0x1021, 0xFFFF, false, false, 0xFFFF, 0xD64E),

				/*
				Name   : "CRC-16/MAXIM"
				Width  : 16
				Poly   : 8005
				Init   : 0000
				RefIn  : True
				RefOut : True
				XorOut : FFFF
				Check  : 44C2
				*/
				new SimpleTestAlgorithm("CRC-16/MAXIM", 16, 0x8005, 0x0000, true, true, 0xFFFF, 0x44C2),

				/*
				Name   : "CRC-16/MCRF4XX"
				Width  : 16
				Poly   : 1021
				Init   : FFFF
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : 6F91
				*/
				new SimpleTestAlgorithm("CRC-16/MCRF4XX", 16, 0x1021, 0xFFFF, true, true, 0x0000, 0x6F91),

				/*
				Name   : "CRC-16/RIELLO"
				Width  : 16
				Poly   : 1021
				Init   : B2AA
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : 63D0
				*/
				new SimpleTestAlgorithm("CRC-16/RIELLO", 16, 0x1021, 0xB2AA, true, true, 0x0000, 0x63D0),

				/*
				Name   : "CRC-16/T10-DIF"
				Width  : 16
				Poly   : 8BB7
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : D0DB
				*/
				new SimpleTestAlgorithm("CRC-16/T10-DIF", 16, 0x8BB7, 0x0000, false, false, 0x0000, 0xD0DB),

				/*
				Name   : "CRC-16/TELEDISK"
				Width  : 16
				Poly   : A097
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 0FB3
				*/
				new SimpleTestAlgorithm("CRC-16/TELEDISK", 16, 0xA097, 0x0000, false, false, 0x0000, 0x0FB3),

				/*
				Name   : "CRC-16/USB"
				Width  : 16
				Poly   : 8005
				Init   : FFFF
				RefIn  : True
				RefOut : True
				XorOut : FFFF
				Check  : B4C8
				*/
				new SimpleTestAlgorithm("CRC-16/USB", 16, 0x8005, 0xFFFF, true, true, 0xFFFF, 0xB4C8),

				/* 
				Name   : "KERMIT"
				Alias  : "CRC-16/CCITT"
				Alias  : "CRC-16/CCITT-TRUE"
				Alias  : "CRC-CCITT"
				Width  : 16
				Poly   : 1021
				Init   : 0000
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : 2189
				XCheck : 8921
				*/
				new SimpleTestAlgorithm("KERMIT", 16, 0x1021, 0x0000, true, true, 0x0000, 0x2189),

				/*
				Name   : "MODBUS"
				Width  : 16
				Poly   : 8005
				Init   : FFFF
				RefIn  : True
				RefOut : True
				XorOut : 0000
				Check  : 4B37
				*/
				new SimpleTestAlgorithm("MODBUS", 16, 0x8005, 0xFFFF, true, true, 0x0000, 0x4B37),

				/*
				Name   : "X-25"
				Alias  : "CRC-16/IBM-SDLC"
				Alias  : "CRC-16/ISO-HDLC"
				Width  : 16
				Poly   : 1021
				Init   : FFFF
				RefIn  : True
				RefOut : True
				XorOut : FFFF
				Check  : 906E
				XCheck : 6E90
				*/
				new SimpleTestAlgorithm("X-25", 16, 0x1021, 0xFFFF, true, true, 0xFFFF, 0x906E),

				/*
				Name   : "XMODEM"
				Alias  : "ZMODEM"
				Alias  : "CRC-16/ACORN"
				Width  : 16
				Poly   : 1021
				Init   : 0000
				RefIn  : False
				RefOut : False
				XorOut : 0000
				Check  : 31C3
				*/
				new SimpleTestAlgorithm("XMODEM", 16, 0x1021, 0x0000, false, false, 0x0000, 0x31C3),

				/*
				Name   : "CRC-24"
				Alias  : "CRC-24/OPENPGP"
				Width  : 24
				Poly   : 864CFB
				Init   : B704CE
				RefIn  : False
				RefOut : False
				XorOut : 000000
				Check  : 21CF02
				*/
				new SimpleTestAlgorithm("CRC-24", 24, 0x864CFB, 0xB704CE, false, false, 0x000000, 0x21CF02),

				/*
				Name   : "CRC-24/FLEXRAY-A"
				Width  : 24
				Poly   : 5D6DCB
				Init   : FEDCBA
				RefIn  : False
				RefOut : False
				XorOut : 000000
				Check  : 7979BD
				*/
				new SimpleTestAlgorithm("CRC-24/FLEXRAY-A", 24, 0x5D6DCB, 0xFEDCBA, false, false, 0x000000, 0x7979BD),

				/*
				Name   : "CRC-24/FLEXRAY-B"
				Width  : 24
				Poly   : 5D6DCB
				Init   : ABCDEF
				RefIn  : False
				RefOut : False
				XorOut : 000000
				Check  : 1F23B8
				*/
				new SimpleTestAlgorithm("CRC-24/FLEXRAY-B", 24, 0x5D6DCB, 0xABCDEF, false, false, 0x000000, 0x1F23B8),

				/*
				Name   : "CRC-32"
				Alias  : "CRC-32/ADCCP"
				Alias  : "PKZIP"
				Width  : 32
				Poly   : 04C11DB7
				Init   : FFFFFFFF
				RefIn  : True
				RefOut : True
				XorOut : FFFFFFFF
				Check  : CBF43926
				*/
				new SimpleTestAlgorithm("CRC-32", 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xCBF43926),

				/*
				Name   : "CRC-32/BZIP2"
				Alias  : "CRC-32/AAL5"
				Alias  : "CRC-32/DECT-B"
				Alias  : "B-CRC-32"
				Width  : 32
				Poly   : 04C11DB7
				Init   : FFFFFFFF
				RefIn  : False
				RefOut : False
				XorOut : FFFFFFFF
				Check  : FC891918
				*/
				new SimpleTestAlgorithm("CRC-32/BZIP2", 32, 0x04C11DB7, 0xFFFFFFFF, false, false, 0xFFFFFFFF, 0xFC891918),

				/*
				Name   : "CRC-32C"
				Alias  : "CRC-32/ISCSI"
				Alias  : "CRC-32/CASTAGNOLI"
				Width  : 32
				Poly   : 1EDC6F41
				Init   : FFFFFFFF
				RefIn  : True
				RefOut : True
				XorOut : FFFFFFFF
				Check  : E3069283
				*/
				new SimpleTestAlgorithm("CRC-32C", 32, 0x1EDC6F41, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xE3069283),

				/*
				Name   : "CRC-32D"
				Width  : 32
				Poly   : A833982B
				Init   : FFFFFFFF
				RefIn  : True
				RefOut : True
				XorOut : FFFFFFFF
				Check  : 87315576
				*/
				new SimpleTestAlgorithm("CRC-32D", 32, 0xA833982B, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0x87315576),

				/*
				Name   : "CRC-32/MPEG-2"
				Width  : 32
				Poly   : 04C11DB7
				Init   : FFFFFFFF
				RefIn  : False
				RefOut : False
				XorOut : 00000000
				Check  : 0376E6E7
				*/
				new SimpleTestAlgorithm("CRC-32/MPEG-2", 32, 0x04C11DB7, 0xFFFFFFFF, false, false, 0x00000000, 0x0376E6E7),

				/*
				Name   : "CRC-32/POSIX"
				Alias  : "CKSUM"
				Width  : 32
				Poly   : 04C11DB7
				Init   : 00000000
				RefIn  : False
				RefOut : False
				XorOut : FFFFFFFF
				Check  : 765E7680
				LCheck : 377A6011
				*/
				new SimpleTestAlgorithm("CRC-32/POSIX", 32, 0x04C11DB7, 0x00000000, false, false, 0xFFFFFFFF, 0x765E7680),

				/*
				Name   : "CRC-32Q"
				Width  : 32
				Poly   : 814141AB
				Init   : 00000000
				RefIn  : False
				RefOut : False
				XorOut : 00000000
				Check  : 3010BF7F
				*/
				new SimpleTestAlgorithm("CRC-32Q", 32, 0x814141AB, 0x00000000, false, false, 0x00000000, 0x3010BF7F),

				/*
				Name   : "JAMCRC"
				Width  : 32
				Poly   : 04C11DB7
				Init   : FFFFFFFF
				RefIn  : True
				RefOut : True
				XorOut : 00000000
				Check  : 340BC6D9
				*/
				new SimpleTestAlgorithm("JAMCRC", 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0x00000000, 0x340BC6D9),

				/*
				Name   : "XFER"
				Width  : 32
				Poly   : 000000AF
				Init   : 00000000
				RefIn  : False
				RefOut : False
				XorOut : 00000000
				Check  : BD0BE338
				*/
				new SimpleTestAlgorithm("XFER", 32, 0x000000AF, 0x00000000, false, false, 0x00000000, 0xBD0BE338),

				/*
				Name   : "CRC-40/GSM"
				Width  : 40
				Poly   : 0004820009
				Init   : 0000000000
				RefIn  : False
				RefOut : False
				XorOut : 0000000000
				Check  : 2BE9B039B9
				*/
				new SimpleTestAlgorithm("CRC-40/GSM", 40, 0x0004820009, 0x0000000000, false, false, 0x0000000000, 0x2BE9B039B9),

				/*
				Name   : "CRC-64"
				Width  : 64
				Poly   : 42F0E1EBA9EA3693
				Init   : 0000000000000000
				RefIn  : False
				RefOut : False
				XorOut : 0000000000000000
				Check  : 6C40DF5F0B497347
				*/
				new SimpleTestAlgorithm("CRC-64", 64, 0x42F0E1EBA9EA3693, 0x0000000000000000, false, false, 0x0000000000000000, 0x6C40DF5F0B497347),

				/*
				Name   : "CRC-64/WE"
				Width  : 64
				Poly   : 42F0E1EBA9EA3693
				Init   : FFFFFFFFFFFFFFFF
				RefIn  : False
				RefOut : False
				XorOut : FFFFFFFFFFFFFFFF
				Check  : 62EC59E3F1A4F00A
				*/
				new SimpleTestAlgorithm("CRC-64/WE", 64, 0x42F0E1EBA9EA3693, 0xFFFFFFFFFFFFFFFF, false, false, 0xFFFFFFFFFFFFFFFF, 0x62EC59E3F1A4F00A)
				/*
				Name   : "CRC-82/DARC"
				Width  : 82
				Poly   : 0308C0111011401440411
				Init   : 000000000000000000000
				RefIn  : True
				RefOut : True
				XorOut : 000000000000000000000
				Check  : 09EA83F625023801FD612
				*/
				//new SimpleTestAlgorithm("CRC-82/DARC", 82, 0x0308C0111011401440411, 0x000000000000000000000, true, true, 0x000000000000000000000, 0x09EA83F625023801FD612)
			};

			foreach (SimpleTestAlgorithm alg in alglist)
			{
				CRCbasic crcbasic = new CRCbasic(alg.Width, alg.Poly, alg.Init, alg.XorOut, alg.RefIn, alg.RefOut);
				CRCoptimal crcopt = new CRCoptimal(alg.Width, alg.Poly, alg.Init, alg.XorOut, alg.RefIn, alg.RefOut);

				crcbasic.ProcessBytes(StandartCheckData);
				crcopt.ProcessBytes(StandartCheckData);

				Debug.Assert(crcbasic.CheckSum == crcopt.CheckSum);
				Debug.Assert(crcbasic.CheckSum == alg.Check);
			}
		}

		static void Main(string[] args)
		{
			BasicTests();
			AugmentedTests();

			Console.WriteLine("All tests passed!!");
			Console.Read();
		}
	}
}