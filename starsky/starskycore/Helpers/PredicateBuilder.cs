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
		///
		/// @see: https://gist.github.com/xwipeoutx/962b205324017c000c75899a8b5016d9
		/// </summary>
		/// <param name="source"></param>
		/// <param name="getDate"></param>
		/// <param name="fromDate"></param>
		/// <param name="toDate"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static IQueryable<T> WhereDateBetween<T>(this IQueryable<T> source,
			Expression<Func<T, DateTime>> getDate,
			DateTime? fromDate, DateTime? toDate)
		{
			if (fromDate == null && toDate == null)
				return source; // The simplest query is no query

			var predicate = DateBetween(fromDate, toDate);
			return source.Where(getDate.Chain(predicate));
		}

		private static Expression<Func<DateTime, bool>> DateBetween(DateTime? fromDate, DateTime? toDate)
		{
			if (toDate == null)
				return date => fromDate <= date;

			if (fromDate == null)
				return date => toDate >= date;

			return date => fromDate <= date && toDate >= date;
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

		
		/// <summary>
		///
		/// @see: https://stackoverflow.com/a/6180943
		/// </summary>
		/// <param name="selectors"></param>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TDestination"></typeparam>
		/// <returns></returns>
		public static Expression<Func<TSource, TDestination>> Combine<TSource, TDestination>(
			params Expression<Func<TSource, TDestination>>[] selectors)
		{
			var param = Expression.Parameter(typeof(TSource), "x");
			var param2 = Expression.Parameter(typeof(TDestination), "x");

			return Expression.Lambda<Func<TSource, TDestination>>(
				Expression.MemberInit(
					Expression.New(typeof(TDestination).GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException("dfsdlfi")),
					from selector in selectors
					let replace = new ParameterReplaceVisitor1(
						selector.Parameters[0], param)
					from binding in ((MemberInitExpression)selector.Body).Bindings
						.OfType<MemberAssignment>()
					select Expression.Bind(binding.Member,
						replace.VisitAndConvert(binding.Expression, "Combine")))
				, param);        
		}
		
		public static Expression<Func<TSource, TTargetB>> Concat<TSource, TTargetA, TTargetB>(
			this Expression<Func<TSource, TTargetA>> mapA, Expression<Func<TSource, TTargetB>> mapB)
			where TTargetB : TTargetA
		{
			var param = Expression.Parameter(typeof(TSource), "i");

			return Expression.Lambda<Func<TSource, TTargetB>>(
				Expression.MemberInit(
					((MemberInitExpression)mapB.Body).NewExpression,
					(new LambdaExpression[] { mapA, mapB }).SelectMany(e =>
					{
						var bindings = ((MemberInitExpression)e.Body).Bindings.OfType<MemberAssignment>();
						return bindings.Select(b =>
						{
							var paramReplacedExp = new ParameterReplaceVisitor(e.Parameters[0], param).VisitAndConvert(b.Expression, "Combine");
							return Expression.Bind(b.Member, paramReplacedExp);
						});
					})),
				param);
		}
		
		
		
		internal class SubstExpressionVisitor : ExpressionVisitor
		{
			private readonly Dictionary<Expression, Expression> _subst = new Dictionary<Expression, Expression>();

			protected override Expression VisitParameter(ParameterExpression node)
			{
				if (_subst.TryGetValue(node, out Expression newValue))
				{
					return newValue;
				}

				return node;
			}

			public Expression this[Expression original]
			{
				get => _subst[original];
				set => _subst[original] = value;
			}
		}
		
		/// <summary>
		///
		/// @see https://stackoverflow.com/a/58261461
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static Expression<Predicate<T>> And<T>(this Expression<Predicate<T>> a, Expression<Predicate<T>> b)
		{
			if (a == null)
				throw new ArgumentNullException(nameof(a));

			if (b == null)
				throw new ArgumentNullException(nameof(b));

			ParameterExpression p = a.Parameters[0];

			SubstExpressionVisitor visitor = new SubstExpressionVisitor();
			visitor[b.Parameters[0]] = p;

			Expression body = Expression.AndAlso(a.Body, visitor.Visit(b.Body));
			return Expression.Lambda<Predicate<T>>(body, p);
		}
		
		public static Expression<Predicate<T>> Or<T>(this Expression<Predicate<T>> a, Expression<Predicate<T>> b)
		{
			if (a == null)
				throw new ArgumentNullException(nameof(a));

			if (b == null)
				throw new ArgumentNullException(nameof(b));

			ParameterExpression p = a.Parameters[0];

			SubstExpressionVisitor visitor = new SubstExpressionVisitor();
			visitor[b.Parameters[0]] = p;

			Expression body = Expression.OrElse(a.Body, visitor.Visit(b.Body));
			return Expression.Lambda<Predicate<T>>(body, p);
		}
		
		private class ParameterReplaceVisitor : ExpressionVisitor
		{
			private readonly ParameterExpression original;
			private readonly ParameterExpression updated;

			public ParameterReplaceVisitor(ParameterExpression original, ParameterExpression updated)
			{
				this.original = original;
				this.updated = updated;
			}

			protected override Expression VisitParameter(ParameterExpression node) => node == original ? updated : base.VisitParameter(node);
		}
		
		class ParameterReplaceVisitor1 : ExpressionVisitor
		{
			private readonly ParameterExpression from, to;
			public ParameterReplaceVisitor1(ParameterExpression from, ParameterExpression to)
			{
				this.from = from;
				this.to = to;
			}
			protected override Expression VisitParameter(ParameterExpression node)
			{
				return node == from ? to : base.VisitParameter(node);
			}
		}
	}
}
