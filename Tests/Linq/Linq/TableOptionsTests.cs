﻿using System;
using System.Linq;

using IBM.Data.DB2;

using JetBrains.Annotations;

#if !NET472
using IBM.Data.DB2.Core;
#endif

using NUnit.Framework;

namespace Tests.Linq
{
	using LinqToDB;
	using LinqToDB.Data;
	using LinqToDB.Mapping;

	[TestFixture]
	public class TableOptionsTests : TestBase
	{
		[Table(IsTemporary = true)]
		[Table(IsTemporary = true, Configuration = ProviderName.SqlServer,  Database = "TestData", Schema = "TestSchema")]
		[Table(IsTemporary = true, Configuration = ProviderName.Sybase,     Database = "TestData")]
		[Table(IsTemporary = true, Configuration = ProviderName.SQLite)]
		[Table(IsTemporary = true, Configuration = ProviderName.PostgreSQL, Database = "TestData", Schema = "test_schema")]
		[Table(IsTemporary = true, Configuration = ProviderName.DB2,                               Schema = "SESSION")]
		[UsedImplicitly]
		class IsTemporaryTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void IsTemporaryFlagTest([DataSources(false)] string context, [Values(true)] bool firstCall)
		{
			using var db = (DataConnection)GetDataContext(context);
			using var table = db.CreateTempTable<IsTemporaryTable>();
			_ = table.ToArray();
		}

		[Table(TableOptions = TableOptions.IsGlobalTemporary)]
		[Table(TableOptions = TableOptions.IsGlobalTemporary, Configuration = ProviderName.DB2, Schema = "SESSION")]
		[UsedImplicitly]
		class IsGlobalTemporaryTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void IsGlobalTemporaryTest([IncludeDataSources(
			ProviderName.DB2,
			ProviderName.Firebird,
			TestProvName.AllSqlServer2005Plus,
			TestProvName.AllSybase)] string context,
			[Values(true)] bool firstCall)
		{
			using var db = (DataConnection)GetDataContext(context);
			using var table = db.CreateTempTable<IsGlobalTemporaryTable>();
			_ = table.ToArray();
		}

		[Table(TableOptions = TableOptions.CreateIfNotExists)]
		[Table(TableOptions = TableOptions.CreateIfNotExists | TableOptions.IsTemporary, Configuration = ProviderName.SqlServer2008)]
		[Table("##temp_table", TableOptions = TableOptions.CreateIfNotExists, Configuration = ProviderName.SqlServer2012)]
		[UsedImplicitly]
		class CreateIfNotExistsTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void CreateIfNotExistsTest([IncludeDataSources(
			true,
			ProviderName.DB2,
			ProviderName.Informix,
			ProviderName.Firebird,
			TestProvName.AllMySql,
			TestProvName.AllOracle,
			ProviderName.PostgreSQL,
			TestProvName.AllSQLite,
			TestProvName.AllSqlServer2005Plus,
			TestProvName.AllSybase)] string context)
		{
			if (context.StartsWith("SqlServer.20") && context.EndsWith(".LinqService"))
				return;

			using var db = GetDataContext(context);

			db.DropTable<CreateIfNotExistsTable>(throwExceptionIfNotExists:false);

			using var table = db.CreateTempTable<CreateIfNotExistsTable>();

			table.Insert(() => new CreateIfNotExistsTable { Id = 1, Value = 2 });

			_ = table.ToArray();
			_ = db.CreateTempTable<CreateIfNotExistsTable>();
		}

		[Test]
		public void CreateTempIfNotExistsTest([IncludeDataSources(
			false,
			ProviderName.DB2,
			ProviderName.Informix,
			ProviderName.Firebird,
			TestProvName.AllMySql,
			TestProvName.AllOracle,
			ProviderName.PostgreSQL,
			TestProvName.AllSQLite,
			TestProvName.AllSqlServer2005Plus,
			TestProvName.AllSybase)] string context)
		{
			if (context.StartsWith("SqlServer.20") && context.EndsWith(".LinqService"))
				return;

			using var db = GetDataContext(context);

			db.DropTable<CreateIfNotExistsTable>(throwExceptionIfNotExists:false);

			using var table = db.CreateTempTable<CreateIfNotExistsTable>(tableOptions:TableOptions.IsTemporary);

			_ = table.ToArray();
			_ = db.CreateTempTable<CreateIfNotExistsTable>();
		}

		[UsedImplicitly]
		class TestTable
		{
			[Column] public int Id    { get; set; }
			[Column] public int Value { get; set; }
		}

		[Test]
		public void IsTemporaryMethodTest([DataSources(false, TestProvName.AllMySql)] string context)
		{
			using var db = GetDataContext(context);

			db.DropTable<TestTable>(tableOptions:TableOptions.IsTemporary | TableOptions.DropIfExists);

			using var table = db.CreateTempTable<TestTable>(tableOptions:TableOptions.IsTemporary);

			_ =
			(
				from t1 in db.GetTable<TestTable>().IsTemporary()
				join t2 in db.GetTable<TestTable>().IsTemporary() on t1.Id equals t2.Id
				join t3 in table on t2.Id equals t3.Id
				select new { t1, t2, t3 }
			)
			.ToList();
		}
	}
}