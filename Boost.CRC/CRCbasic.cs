using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;

namespace Boost.CRC
{
	/// <summary>
	/// Теоретическая модель CRC-вычислителя
	/// </summary>
	/// <remarks>
	/// Основана на модели CRC алгоритма Rocksoft.
	/// Ввиду отсутствия на C#-целочисленных параметров шаблонов пришлось отойти от реализации алгорима в библиотеке boost.
	/// По заверениям вики CRC-128/256 вытеснили алгоритмы хэширования. Потому 64-битного регистра для вычислений нам должно хватить.
	/// </remarks>
	public class CRCbasic
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
		/// Этот параметр определяет исходное содержимое регистра на момент запуска вычислений.
		/// </summary>
		/// <remarks>Данный параметр указывается шестнадцатеричным числом.</remarks>
		public readonly ulong InitialRemainder;

		/// <summary>
		/// W битное значение, обозначаемое шестнадцатеричным числом.
		/// </summary>
		/// <remarks>
		/// Оно комбинируется с конечным содержимым регистра (после стадии RefOut), прежде чем будет получено окончательное значение контрольной суммы.
		/// </remarks>
		public readonly ulong FinalXorValue;

		/// <summary>
		/// Логический параметр.
		/// </summary>
		/// <remarks>
		/// Если он имеет значение False, байты сообщения обрабатываются, начиная с 7 бита, который считается наиболее значащим, а наименее значащим
		/// считается бит 0 (сдвиг налево). Если параметр имеет значение True («Истина»), то каждый байт перед обработкой обращается (сдвиг направо).
		/// </remarks>
		public readonly bool ReflectInput;

		/// <summary>
		/// Логический параметр.
		/// </summary>
		/// <remarks>
		/// Если он имеет значение False, то конечное содержимое регистра сразу передается на стадию XorOut, в противном случае, когда параметр имеет
		/// значение True, содержимое регистра обращается перед передачей на следующую стадию вычислений. 
		/// </remarks>
		public readonly bool ReflectRemainder;


		/// <summary>
		/// текущее значение остатка
		/// </summary>
		private ulong Remainder;

		/// <summary>
		/// текущий остаток
		/// </summary>
		public ulong InterimRemainder
		{
			get
			{
				return Remainder & masking_type.SigBits;
			}
		}

		private readonly MaskUint masking_type;

		/// <summary>
		/// Конструктор ЦРЦ-вычислителя
		/// </summary>
		public CRCbasic(int Bits, ulong TruncPoly,  ulong InitialRemainder=0, ulong FinalXorValue=0, bool ReflectInput=false,
			bool ReflectRemainder=false)
		{
			this.Bits=Bits;
			this.TruncPoly=TruncPoly;
			this.InitialRemainder=InitialRemainder;
			Remainder = InitialRemainder;
			this.FinalXorValue=FinalXorValue;
			this.ReflectInput=ReflectInput;
			this.ReflectRemainder=ReflectRemainder;
			masking_type = new MaskUint(Bits);
		}

		/// <summary>
		/// обработка бита
		/// </summary>
		/// <param name="bit"></param>
		public void ProcessBit(bool bit)
		{
			// compare the new bit with the remainder's highest
			Remainder ^= (bit ? masking_type.HighBitMask : 0);

			// a full polynominal division step is done when the highest bit is one
			bool DoPolyDiv = (Remainder & masking_type.HighBitMask) != 0;

			// shift out the highest bit
			Remainder <<= 1;

			// carry out the division, if needed
			if (DoPolyDiv)
				Remainder ^= TruncPoly;
		}

		/// <summary>
		/// обработка нескольких бит внутри байта
		/// </summary>
		/// <param name="bits"></param>
		/// <param name="BitCount">число бит. не более 8</param>
		public void ProcessBits(byte bits, int BitCount)
		{
			// ignore the bits above the ones we want
			bits <<= Limits.CHAR_BIT - BitCount;

			// compute the CRC for each bit, starting with the upper ones
			const byte ByteHighBitMask = MaskUint.ByteHighBitMask;

			for (int i = BitCount; i > 0; --i, bits <<= 1)
				ProcessBit((bits & ByteHighBitMask) != 0);
		}

		/// <summary>
		/// обработка одного байта
		/// </summary>
		/// <param name="Byte"></param>
		public void ProcessByte(byte OneByte)
		{
			if (ReflectInput)
				OneByte = (byte)Detail.Reflector.reflect(OneByte, 8);

			ProcessBits(OneByte, Limits.CHAR_BIT);
		}

		/// <summary>
		/// обработка массива байт
		/// </summary>
		/// <param name="buffer"></param>
		public void ProcessBytes(params byte[] buffer)
		{
			foreach (byte b in buffer)
				ProcessByte(b);
		}

		/// <summary>
		/// обработка массива байт
		/// </summary>
		/// <param name="buffer"></param>
		public void ProcessBytes(byte[] buffer, int offset, int size)
		{
			for (int x = 0; x < size; x++)
				ProcessByte(buffer[offset + x]);
		}

		/// <summary>
		/// обработка массива байт
		/// </summary>
		/// <param name="buffer"></param>
		public void ProcessBytes(byte[] buffer, int size)
		{
			for (int x = 0; x < size; x++)
				ProcessByte(buffer[x]);
		}


		/// <summary>
		/// сброс вычислителя в начальное состояние
		/// </summary>
		public void Reset()
		{
			Reset(InitialRemainder);
		}

		/// <summary>
		/// сброс вычислителя в известное состояние
		/// </summary>
		/// <param name="NewRemainder"></param>
		public void Reset(ulong NewRemainder)
		{
			Remainder = NewRemainder;
		}

		/// <summary>
		/// выдаёт контрольную сумму
		/// </summary>
		public ulong CheckSum
		{
			get
			{
				return ((ReflectRemainder ? Detail.Reflector.reflect(Remainder, Bits) : Remainder) ^ FinalXorValue) & masking_type.SigBits;
			}
		}
	}
}
