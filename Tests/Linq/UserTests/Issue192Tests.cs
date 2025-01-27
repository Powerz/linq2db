﻿using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

using NUnit.Framework;

namespace Tests.UserTests
{
	[TestFixture]
	public class Issue192Tests : TestBase
	{
		[Table(Name = "TypeConvertTable")]
		public class TypeConvertTable
		{
			[Column(Length = 50), NotNull]
			public string Name   { get; set; } = null!;

			[Column(DataType = DataType.Char), NotNull]
			public bool BoolValue { get; set; }

			[Column(DataType = DataType.VarChar, Length = 50), Nullable]
			public Guid? GuidValue { get; set; }

			public override string ToString()
			{
				return string.Format("{0} {1} {2}", Name, BoolValue, GuidValue);
			}

			public override bool Equals(object? obj)
			{
				var e = obj as TypeConvertTable;

				if (e == null)
					return false;

				return
					Name      == e.Name &&
					BoolValue == e.BoolValue &&
					GuidValue == e.GuidValue;
			}

			public override int GetHashCode()
			{
				return 0;
			}

		}

		[Table(Name = "TypeConvertTable")]
		public class TypeConvertTableRaw
		{
			[Column(Length = 50), NotNull]
			public string Name   { get; set; } = null!;

			[Column(DataType = DataType.Char), NotNull]
			public char BoolValue { get; set; }

			[Column(DataType = DataType.VarChar, Length = 50), Nullable]
			public string? GuidValue { get; set; }

		}

		[Test]
		public void Test([DataSources] string context)
		{
			var ms = new MappingSchema();

			ms.SetConvertExpression<bool,   DataParameter>(_ => DataParameter.Char(null, _ ? 'Y' : 'N'));
			ms.SetConvertExpression<char,   bool>(_ => _ == 'Y');
			ms.SetConvertExpression<string, bool>(_ => _.Trim() == "Y");

			ms.SetConvertExpression<Guid?,  DataParameter>(_ => DataParameter.VarChar(null, _.ToString()));
			ms.SetConvertExpression<string, Guid?>(_ => Guid.Parse(_));

			using (var db  = GetDataContext(context, ms))
			using (var tbl = db.CreateLocalTable<TypeConvertTable>())
			{
				var notVerified = new TypeConvertTable
				{
					Name      = "NotVerified",
					BoolValue = false,
					GuidValue = TestData.Guid1
				};

				var verified = new TypeConvertTable
				{
					Name      = "Verified",
					BoolValue = true,
					GuidValue = TestData.Guid2
				};

				db.Insert(notVerified, tbl.TableName);
				db.Insert(verified,    tbl.TableName);


				Assert.AreEqual(1, db.GetTable<TypeConvertTableRaw>().TableName(tbl.TableName).Count(_ => _.BoolValue == 'N'));
				Assert.AreEqual(1, db.GetTable<TypeConvertTableRaw>().TableName(tbl.TableName).Count(_ => _.BoolValue == 'Y'));
				Assert.AreEqual(1, db.GetTable<TypeConvertTableRaw>().TableName(tbl.TableName).Count(_ => _.GuidValue == verified.GuidValue.ToString()));

				Assert.AreEqual(notVerified, tbl.First(_ => _.BoolValue == false));
				Assert.AreEqual(verified,    tbl.First(_ => _.BoolValue == true));

				Assert.AreEqual(notVerified, tbl.First(_ => _.BoolValue != true));
				Assert.AreEqual(verified,    tbl.First(_ => _.BoolValue != false));

				Assert.AreEqual(notVerified, tbl.First(_ => !_.BoolValue));
				Assert.AreEqual(verified,    tbl.First(_ =>  _.BoolValue));

				Assert.AreEqual(notVerified, tbl.First(_ => _.BoolValue.Equals(false)));
				Assert.AreEqual(verified,    tbl.First(_ => _.BoolValue.Equals(true)));

				Assert.AreEqual(notVerified, tbl.First(_ => !_.BoolValue.Equals(true)));
				Assert.AreEqual(verified,    tbl.First(_ => !_.BoolValue.Equals(false)));

				Assert.AreEqual(notVerified, tbl.First(_ => _.GuidValue == notVerified.GuidValue));
				Assert.AreEqual(verified,    tbl.First(_ => _.GuidValue == verified   .GuidValue));
			}
		}
	}
}
