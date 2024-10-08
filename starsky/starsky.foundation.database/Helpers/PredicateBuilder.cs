using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace starsky.foundation.database.Helpers
{

	/// <summary>
	/// to combine EF queries
	/// </summary>
	public static class PredicateBuilder
	{
		/// <summary>
		/// Query setup positive 
		/// </summary>
		/// <typeparam name="T">type</typeparam>
		/// <returns>Expression</returns>
		public static Expression<Func<T, bool>> True<T>() { return f => true; }

		/// <summary>
		/// Query setup negative 
		/// </summary>
		/// <typeparam name="T">type</typeparam>
		/// <returns>Expression</returns>
		public static Expression<Func<T, bool>> False<T>() { return f => false; }

		/// <summary>
		/// Or Expression
		/// </summary>
		/// <param name="expr1">first</param>
		/// <param name="expr2">second</param>
		/// <typeparam name="T">object type</typeparam>
		/// <returns>Expression</returns>
		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters);
			return Expression.Lambda<Func<T, bool>>
				(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		/// <summary>
		/// And Expression
		/// </summary>
		/// <param name="expr1">first</param>
		/// <param name="expr2">second</param>
		/// <typeparam name="T">object type</typeparam>
		/// <returns>Expression</returns>
		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
		}

		/// <summary>
		/// @see: web.archive.org/web/http://blogs.msdn.com/b/meek/archive/2008/05/02/linq-to-entities-combining-predicates.aspx
		/// </summary>
		/// <param name="predicates"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Expression<Func<T, bool>> OrLoop<T>(List<Expression<Func<T, bool>>> predicates)
		{
			var predicate = False<T>();

			for ( var i = 0; i < predicates.Count; i++ )
			{
				if ( i == 0 )
				{

					predicate = predicates[i];
				}
				else
				{
					var item2 = predicates[i];
					predicate = predicate.Or(item2);
				}
			}

			return predicate;
		}

		/// <summary>
		/// Combine two queries
		/// @see https://stackoverflow.com/a/457328
		/// </summary>
		/// <param name="expr1">first</param>
		/// <param name="expr2">second</param>
		/// <typeparam name="T">object type</typeparam>
		/// <returns>Expression</returns>
		public static Expression<Func<T, bool>> AndAlso<T>(
			this Expression<Func<T, bool>> expr1,
			Expression<Func<T, bool>> expr2)
		{
			var parameter = Expression.Parameter(typeof(T));

			var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
			var left = leftVisitor.Visit(expr1.Body);

			var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
			var right = rightVisitor.Visit(expr2.Body);

			return Expression.Lambda<Func<T, bool>>(
				Expression.AndAlso(left, right), parameter);
		}

		private sealed class ReplaceExpressionVisitor
			: ExpressionVisitor
		{
			private readonly Expression _oldValue;
			private readonly Expression _newValue;

			public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
			{
				_oldValue = oldValue;
				_newValue = newValue;
			}

			public override Expression Visit(Expression? node)
			{
				if ( node == _oldValue )
				{
					return _newValue;
				}

				return base.Visit(node)!;
			}
		}

	}
}
