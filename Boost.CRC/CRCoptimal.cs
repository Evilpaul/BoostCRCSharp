using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boost.Detail;

namespace Boost.CRC
{
	/// <summary>
	/// Оптимизированная модель CRC-вычислителя
	/// </summary>
	/// /// <remarks>
	/// Основана на модели CRC алгоритма Rocksoft.
	/// </remarks>
	public class CRCoptimal<T> where T: new()
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
		/// рабочие маски
		/// </summary>
		private readonly MaskUint<T> masking_type;

		private readonly CRChelper<T> helper_type;

		private readonly CRChelper<T> reflect_out_type;

		private readonly CRCtable<T> crc_table_type;

		/// <summary>
		/// Конструктор ЦРЦ-вычислителя
		/// </summary>
		public CRCoptimal(int Bits, T TruncPoly,  T InitialRemainder, T FinalXorValue, bool ReflectInput=false, bool ReflectRemainder=false)
		{
			this.Bits=Bits;
			this.ReflectInput=ReflectInput;
			this.FinalXorValue=FinalXorValue;
			this.ReflectRemainder=ReflectRemainder;
			this.TruncPoly=TruncPoly;
			this.InitialRemainder=InitialRemainder;

			masking_type = new MaskUint<T>(Bits);
			helper_type = new CRChelper<T>(Bits, ReflectInput);

			reflect_out_type = new CRChelper<T>(Bits, (ReflectRemainder != ReflectInput));

			Remainder =helper_type.reflect(InitialRemainder);

			crc_table_type = new CRCtable<T>(Bits, TruncPoly, ReflectInput);

			crc_table_type.InitTable();
		}

		public void ProcessByte(byte p)
		{
			// Compare the new byte with the remainder's higher bits to
			// get the new bits, shift out the remainder's current higher
			// bits, and update the remainder with the polynominal division
			// of the new bits.

			byte byte_index = helper_type.index(Remainder, p);

			dynamic dynRemainder = helper_type.shift(Remainder);
			dynRemainder ^= crc_table_type.Table[byte_index];
			Remainder = (T)dynRemainder;
		}

		public void ProcessBytes(params byte[] mas)
		{
			// Recompute the CRC for each byte passed
			foreach (byte p in mas)
				ProcessByte(p);
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
		/// сброс вычислителя в известное состояние
		/// </summary>
		/// <param name="NewRemainder"></param>
		public void Reset(T NewRemainder)
		{
			Remainder = helper_type.reflect(NewRemainder);
		}

		/// <summary>
		/// текущий остаток
		/// </summary>
		public T InterimRemainder
		{
			get
			{
				// Interim remainder should be _un_-reflected, so we have to undo it.
				dynamic ret = helper_type.reflect(Remainder);

				return ret & masking_type.SigBits;
			}
		}

		/// <summary>
		/// выдаёт контрольную сумму
		/// </summary>
		public T CheckSum
		{
			get
			{
				dynamic ret=reflect_out_type.reflect(Remainder);

				ret^= FinalXorValue;

				return (T)(ret & masking_type.SigBits);
			}
		}
	}
}
