using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;
using MiscUtil;

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
	public class CRCbasic<T> where T: new()
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
		/// Этот параметр определяет исходное содержимое регистра на момент запуска вычислений.
		/// </summary>
		/// <remarks>Данный параметр указывается шестнадцатеричным числом.</remarks>
		public readonly T InitialRemainder;

		/// <summary>
		/// W битное значение, обозначаемое шестнадцатеричным числом.
		/// </summary>
		/// <remarks>
		/// Оно комбинируется с конечным содержимым регистра (после стадии RefOut), прежде чем будет получено окончательное значение контрольной суммы.
		/// </remarks>
		public readonly T FinalXorValue;

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
		private T Remainder;

		/// <summary>
		/// текущий остаток
		/// </summary>
		public T InterimRemainder
		{
			get
			{
				return Operator<T>.And(Remainder, masking_type.SigBits);
			}
		}

		private readonly MaskUint<T> masking_type;

		/// <summary>
		/// Конструктор ЦРЦ-вычислителя
		/// </summary>
		public CRCbasic(int Bits, T TruncPoly, T InitialRemainder, T FinalXorValue, bool ReflectInput = false,
			bool ReflectRemainder=false)
		{
			this.Bits=Bits;
			this.TruncPoly=TruncPoly;
			this.InitialRemainder=InitialRemainder;
			Remainder = InitialRemainder;
			this.FinalXorValue=FinalXorValue;
			this.ReflectInput=ReflectInput;
			this.ReflectRemainder=ReflectRemainder;
			masking_type = new MaskUint<T>(Bits);
		}

		/// <summary>
		/// обработка бита
		/// </summary>
		/// <param name="bit"></param>
		public void ProcessBit(bool bit)
		{
			// compare the new bit with the remainder's highest
			if (bit)
				Remainder = Operator<T>.Xor(Remainder, masking_type.HighBitMask);

			// a full polynominal division step is done when the highest bit is one
			bool DoPolyDiv = Operator<T>.NotEqual(
				 Operator<T>.And(Remainder, masking_type.HighBitMask), Operator<T>.Zero
				 );

			// shift out the highest bit
			Remainder = Operator<T>.LeftShift(Remainder, 1);

			// carry out the division, if needed
			if (DoPolyDiv)
				Remainder = Operator<T>.Xor(Remainder, TruncPoly);
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
			const byte ByteHighBitMask = MaskUint<T>.ByteHighBitMask;

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
				OneByte = (byte)Reflector<byte>.reflect(OneByte, 8);

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
		public void Reset(T NewRemainder)
		{
			Remainder = NewRemainder;
		}

		/// <summary>
		/// выдаёт контрольную сумму
		/// </summary>
		public T CheckSum
		{
			get
			{
				T RetVal = ReflectRemainder ? Reflector<T>.reflect(Remainder, Bits) : Remainder;

				return Operator<T>.And(
					Operator<T>.Xor(RetVal, FinalXorValue),
					masking_type.SigBits);
			}
		}
	}
}
