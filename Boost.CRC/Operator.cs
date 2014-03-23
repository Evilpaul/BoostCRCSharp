using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using MiscUtil.Linq;

namespace MiscUtil
{
	/// <summary>
	/// обобщённые математические операции
	/// </summary>
	/// <remarks>За основу был взят проект MiscUtil (Jon Skeet and Marc Gravell. skeet@pobox.com).
	/// Были добавлены математические операции требуемые для рассчёта CRC. выкинуты остальные
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	static class Operator<T>
	{
		/*
		T--;
		*/

		public static readonly Func<T, T, T> And;
		public static readonly Func<T, T, T> Xor;
		public static readonly Func<T, T, T> Or;
		public static readonly Func<T, T> Not;
		public static readonly Func<T, int, T> LeftShift;
		public static readonly Func<T, int, T> RightShift;
		public static readonly Func<T, T, bool> NotEqual;
		public static readonly Func<T, T, bool> Equal;

		/// <summary>
		/// значение нулевого типа
		/// </summary>
		public static readonly T Zero=default(T);

		static Operator()
		{
			And = ExpressionUtil.CreateExpression<T, T, T>(Expression.And);
			Xor = ExpressionUtil.CreateExpression<T, T, T>(Expression.ExclusiveOr);
			LeftShift = ExpressionUtil.CreateExpression<T, int, T>(Expression.LeftShift);
			RightShift = ExpressionUtil.CreateExpression<T, int, T>(Expression.RightShift);
			NotEqual = ExpressionUtil.CreateExpression<T, T, bool>(Expression.NotEqual);
			Equal = ExpressionUtil.CreateExpression<T, T, bool>(Expression.Equal);
			Or = ExpressionUtil.CreateExpression<T, T, T>(Expression.Or);
			Not=ExpressionUtil.CreateExpression<T, T>(Expression.Not);
		}
	}

	/// <summary>
	/// обобщённые математические операции. Используется для приведения типов
	/// </summary>
	/// <typeparam name="TValue"></typeparam>
	/// <typeparam name="TResult"></typeparam>
	/// <remarks>За основу был взят проект MiscUtil (Jon Skeet and Marc Gravell. skeet@pobox.com).
	/// Выкинуто лишнее</remarks>
	static class Operator<TValue, TResult>
	{
		public static Func<TValue, TResult> Convert;

		static Operator()
		{
			Convert=ExpressionUtil.CreateExpression<TValue, TResult>(body => Expression.Convert(body, typeof(TResult)));
		}
	}
}
