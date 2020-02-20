using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace starskycore.Helpers
{

	/// <summary>
	/// to combine EF queries
	/// </summary>
	public static class PredicateBuilder
	{
		public static Expression<Func<T, bool>> True<T> ()  { return f => true;  }
		public static Expression<Func<T, bool>> False<T> () { return f => false; }
 
		public static Expression<Func<T, bool>> Or<T> (this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
			return Expression.Lambda<Func<T, bool>>
				(Expression.OrElse (expr1.Body, invokedExpr), expr1.Parameters);
		}
		
		public static Expression<Func<T, bool>> And<T> (this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke (expr2, expr1.Parameters.Cast<Expression> ());
			return Expression.Lambda<Func<T, bool>>
				(Expression.AndAlso (expr1.Body, invokedExpr), expr1.Parameters);
		}
		
		/// <summary>
		/// @see https://stackoverflow.com/a/457328
		/// </summary>
		/// <param name="expr1"></param>
		/// <param name="expr2"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Expression<Func<T, bool>> AndAlso<T>(
			this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var parameter = Expression.Parameter(typeof (T));

			var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
			var left = leftVisitor.Visit(expr1.Body);

			var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
			var right = rightVisitor.Visit(expr2.Body);

			return Expression.Lambda<Func<T, bool>>(
				Expression.AndAlso(left, right), parameter);
		}

		private class ReplaceExpressionVisitor
			: ExpressionVisitor
		{
			private readonly Expression _oldValue;
			private readonly Expression _newValue;

			public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
			{
				_oldValue = oldValue;
				_newValue = newValue;
			}

			public override Expression Visit(Expression node)
			{
				if (node == _oldValue)
					return _newValue;
				return base.Visit(node);
			}
		}
 
		public static Expression<Func<TIn, TOut>> Chain<TIn, TInterstitial, TOut>(
			this Expression<Func<TIn, TInterstitial>> inner,
			Expression<Func<TInterstitial, TOut>> outer)
		{
			var visitor = new SwapVisitor(outer.Parameters[0], inner.Body);
			return Expression.Lambda<Func<TIn, TOut>>(visitor.Visit(outer.Body), inner.Parameters);
		}
		
		internal class SwapVisitor : ExpressionVisitor
		{
			private readonly Expression _source, _replacement;

			public SwapVisitor(Expression source, Expression replacement)
			{
				_source = source;
				_replacement = replacement;
			}

			public override Expression Visit(Expression node)
			{
				return node == _source ? _replacement : base.Visit(node);
			}
		}
	}
}
