﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using OleDbType = LinqToDB.DataProvider.OleDbProviderAdapter.OleDbType;

namespace LinqToDB.DataProvider.Access
{
	using System.Data.Common;
	using Common;
	using Data;
	using Mapping;
	using SchemaProvider;
	using SqlProvider;

	public class AccessOleDbDataProvider : DynamicDataProviderBase<OleDbProviderAdapter>
	{
		public AccessOleDbDataProvider() : base(ProviderName.Access, MappingSchemaInstance, OleDbProviderAdapter.GetInstance())
		{
			SqlProviderFlags.AcceptsTakeAsParameter           = false;
			SqlProviderFlags.IsSkipSupported                  = false;
			SqlProviderFlags.IsCountSubQuerySupported         = false;
			SqlProviderFlags.IsInsertOrUpdateSupported        = false;
			SqlProviderFlags.TakeHintsSupported               = TakeHints.Percent;
			SqlProviderFlags.IsCrossJoinSupported             = false;
			SqlProviderFlags.IsInnerJoinAsCrossSupported      = false;
			SqlProviderFlags.IsDistinctOrderBySupported       = false;
			SqlProviderFlags.IsDistinctSetOperationsSupported = false;
			SqlProviderFlags.IsParameterOrderDependent        = true;
			SqlProviderFlags.IsUpdateFromSupported            = false;
			SqlProviderFlags.DefaultMultiQueryIsolationLevel  = IsolationLevel.Unspecified;

			SetCharField            ("DBTYPE_WCHAR", (r, i) => r.GetString(i).TrimEnd(' '));
			SetCharFieldToType<char>("DBTYPE_WCHAR", DataTools.GetCharExpression);

			SetProviderField<DbDataReader, TimeSpan, DateTime>((r, i) => r.GetDateTime(i) - new DateTime(1899, 12, 30));

			_sqlOptimizer = new AccessSqlOptimizer(SqlProviderFlags);
		}

		public override TableOptions SupportedTableOptions => TableOptions.None;

		public override ISqlBuilder CreateSqlBuilder(MappingSchema mappingSchema)
		{
			return new AccessOleDbSqlBuilder(this, mappingSchema, GetSqlOptimizer(), SqlProviderFlags);
		}

		readonly ISqlOptimizer _sqlOptimizer;

		public override ISqlOptimizer GetSqlOptimizer()
		{
			return _sqlOptimizer;
		}

		public override ISchemaProvider GetSchemaProvider()
		{
			return new AccessOleDbSchemaProvider(this);
		}

#if NET6_0_OR_GREATER
		public override void SetParameter(DataConnection dataConnection, DbParameter parameter, string name, DbDataType dataType, object? value)
		{
			if (value is DateOnly d)
				value = d.ToDateTime(TimeOnly.MinValue);

			base.SetParameter(dataConnection, parameter, name, dataType, value);
		}
#endif

		protected override void SetParameterType(DataConnection dataConnection, DbParameter parameter, DbDataType dataType)
		{
			OleDbType? type = null;
			switch (dataType.DataType)
			{
				case DataType.DateTime  :
				case DataType.DateTime2 : type = OleDbType.Date        ; break;
				case DataType.Text      : type = OleDbType.LongVarChar ; break;
				case DataType.NText     : type = OleDbType.LongVarWChar; break;
			}

			if (type != null)
			{
				var param = TryGetProviderParameter(dataConnection, parameter);
				if (param != null)
				{
					Adapter.SetDbType(param, type.Value);
					return;
				}
			}

			switch (dataType.DataType)
			{
				// "Data type mismatch in criteria expression" fix for culture-aware number decimal separator
				// unfortunatelly, regular fix using ExecuteScope=>InvariantCultureRegion
				// doesn't work for all situations
				case DataType.Decimal   :
				case DataType.VarNumeric: parameter.DbType = DbType.AnsiString; return;
				case DataType.DateTime  :
				case DataType.DateTime2 : parameter.DbType = DbType.DateTime;   return;
				case DataType.Text      : parameter.DbType = DbType.AnsiString; return;
				case DataType.NText     : parameter.DbType = DbType.String;     return;
			}

			base.SetParameterType(dataConnection, parameter, dataType);
		}

		static readonly MappingSchema MappingSchemaInstance = new AccessMappingSchema.OleDbMappingSchema();

		#region BulkCopy

		public override BulkCopyRowsCopied BulkCopy<T>(
			ITable<T> table, BulkCopyOptions options, IEnumerable<T> source)
		{

			return new AccessBulkCopy().BulkCopy(
				options.BulkCopyType == BulkCopyType.Default ? AccessTools.DefaultBulkCopyType : options.BulkCopyType,
				table,
				options,
				source);
		}

		public override Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			ITable<T> table, BulkCopyOptions options, IEnumerable<T> source, CancellationToken cancellationToken)
		{

			return new AccessBulkCopy().BulkCopyAsync(
				options.BulkCopyType == BulkCopyType.Default ? AccessTools.DefaultBulkCopyType : options.BulkCopyType,
				table,
				options,
				source,
				cancellationToken);
		}

#if NATIVE_ASYNC
		public override Task<BulkCopyRowsCopied> BulkCopyAsync<T>(
			ITable<T> table, BulkCopyOptions options, IAsyncEnumerable<T> source, CancellationToken cancellationToken)
		{

			return new AccessBulkCopy().BulkCopyAsync(
				options.BulkCopyType == BulkCopyType.Default ? AccessTools.DefaultBulkCopyType : options.BulkCopyType,
				table,
				options,
				source,
				cancellationToken);
		}
#endif

		#endregion
	}
}
