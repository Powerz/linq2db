﻿using System.Linq.Expressions;

namespace LinqToDB.Linq.Builder
{
	using LinqToDB.Expressions;
	using SqlQuery;

	using static LinqToDB.Reflection.Methods.LinqToDB.Merge;

	internal partial class MergeBuilder
	{
		internal class UpdateWhenNotMatchedBySource : MethodCallBuilder
		{
			protected override bool CanBuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
			{
				return methodCall.IsSameGenericMethod(UpdateWhenNotMatchedBySourceAndMethodInfo);
			}

			protected override IBuildContext BuildMethodCall(ExpressionBuilder builder, MethodCallExpression methodCall, BuildInfo buildInfo)
			{
				// UpdateWhenNotMatchedBySourceAnd(merge, searchCondition, setter)
				var mergeContext = (MergeContext)builder.BuildSequence(new BuildInfo(buildInfo, methodCall.Arguments[0]));

				var statement = mergeContext.Merge;
				var operation = new SqlMergeOperationClause(MergeOperationType.UpdateBySource);
				statement.Operations.Add(operation);

				Expression predicate = methodCall.Arguments[1];
				Expression setter = methodCall.Arguments[2];

				UpdateBuilder.BuildSetter(
					builder,
					buildInfo,
					(LambdaExpression)setter.Unwrap(),
					mergeContext,
					operation.Items,
					mergeContext);

				if (!predicate.IsNullValue())
				{
					var condition = (LambdaExpression)predicate.Unwrap();

					operation.Where = BuildSearchCondition(builder, statement, mergeContext.TargetContext, null, condition);
				}

				return mergeContext;
			}
		}
	}
}
