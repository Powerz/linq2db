﻿using System.Collections.Generic;
using System.Linq;
using LinqToDB;
using LinqToDB.Mapping;
using NUnit.Framework;

namespace Tests.UserTests
{
	[TestFixture]
	public class Issue825Tests : TestBase
	{
		[Table(Name = "Child")]
		public class Child825
		{
			[PrimaryKey, Identity, Column("ChildID")]
			public int Id { get; set; }

			[Column("ParentID"), NotNull]
			public int ParentId { get; set; }

			[Association(ThisKey = "ParentId", OtherKey = "Id", CanBeNull = false)]
			public Parent825 Parent { get; set; } = null!;
		}

		[Table(Name = "Parent")]
		public class Parent825
		{
			[PrimaryKey, Identity, Column("ParentID")]
			public int Id { get; set; }

			[Association(ThisKey = "Id", OtherKey = "ParentId", CanBeNull = true)]
			public IList<ParentPermission> ParentPermissions { get; set; } = null!;

			[Association(ThisKey = "Id", OtherKey = "ParentId", CanBeNull = true)]
			public IList<Child825> Childs { get; set; } = null!;
		}

		[Table(Name = "GrandChild")]
		public class ParentPermission
		{
			[PrimaryKey, Identity, Column("GrandChildID")]
			public int Id { get; set; }

			[Column("ParentID"), NotNull]
			public int ParentId { get; set; }

			[Column("ChildID"), NotNull]
			public int UserId { get; set; }
		}

		[Test]
		public void Test([DataSources] string context)
		{
			using (var db = GetDataContext(context))
			{
				var userId  = 32;
				var childId = 32;

				//Configuration.Linq.OptimizeJoins = false;

				var query = db.GetTable<Parent825>()
					.Where(p => p.ParentPermissions.Any(permission => permission.UserId == userId))
					.SelectMany(parent => parent.Childs)
					.Where(child => child.Id == childId)
					.Select(child => child.Parent);

				var result = query.ToList();

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual(3, result[0].Id);
			}
		}
	}
}
