﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LinqToDB.Linq;
using LinqToDB.SqlQuery;

namespace Tests
{
	public static class QueryUtils
	{
		public static SqlStatement GetStatement<T>(this IQueryable<T> query)
		{
			var eq = (IExpressionQuery)query;
			var expression = eq.Expression;
			var info = Query<T>.GetQuery(eq.DataContext, ref expression, out _);

			InitParameters(eq, info, expression);

			return info.GetQueries().Single().Statement;
		}

		private static void InitParameters<T>(IExpressionQuery eq, Query<T> info, Expression expression)
		{
			eq.DataContext.GetQueryRunner(info, 0, expression, null, null).GetSqlText();
		}

		public static SelectQuery GetSelectQuery<T>(this IQueryable<T> query)
		{
			return query.GetStatement().SelectQuery!;
		}

		public static IEnumerable<SelectQuery> EnumQueries<T>([NoEnumeration] this IQueryable<T> query)
		{
			var selectQuery = query.GetSelectQuery();
			var information = new QueryInformation(selectQuery);
			return information.GetQueriesParentFirst();
		}

		public static IEnumerable<SqlJoinedTable> EnumJoins(this SelectQuery query)
		{
			return query.From.Tables.SelectMany(t => t.Joins);
		}

		public static SqlSearchCondition GetWhere<T>(this IQueryable<T> query)
		{
			return GetSelectQuery(query).Where.SearchCondition;
		}

		public static SqlSearchCondition GetWhere(this SelectQuery selectQuery)
		{
			return selectQuery.Where.SearchCondition;
		}

		public static SqlTableSource GetTableSource(this SelectQuery selectQuery)
		{
			return selectQuery.From.Tables.Single();
		}

		public static SqlTableSource GetTableSource<T>(this IQueryable<T> query)
		{
			return GetSelectQuery(query).From.Tables.Single();
		}
	}
}
