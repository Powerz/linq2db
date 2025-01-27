﻿using System.Linq;

using LinqToDB;

using NUnit.Framework;

namespace Tests.Linq
{
	using Model;

	[TestFixture]
	public class ElementOperationTests : TestBase
	{
		[Test]
		public void First([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(
					   Parent.OrderByDescending(p => p.ParentID).First().ParentID,
					db.Parent.OrderByDescending(p => p.ParentID).First().ParentID);
		}

		[Test]
		public void FirstWhere([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(2, db.Parent.First(p => p.ParentID == 2).ParentID);
		}

		[Test]
		public void FirstOrDefault([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.IsNull((from p in db.Parent where p.ParentID == 100 select p).FirstOrDefault());
		}

		[Test]
		public void FirstOrDefaultWhere([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(2, db.Parent.FirstOrDefault(p => p.ParentID == 2)!.ParentID);
		}

		[Test]
		public void Single([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(1, db.Parent.Where(p => p.ParentID == 1).Single().ParentID);
		}

		[Test]
		public void SingleWhere([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(2, db.Parent.Single(p => p.ParentID == 2).ParentID);
		}

		[Test]
		public void SingleOrDefault([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.IsNull((from p in db.Parent where p.ParentID == 100 select p).SingleOrDefault());
		}

		[Test]
		public void SingleOrDefaultWhere([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(2, db.Parent.SingleOrDefault(p => p.ParentID == 2)!.ParentID);
		}

		[Test]
		public void FirstOrDefaultScalar([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				Assert.AreEqual(
					   Parent.OrderBy(p => p.ParentID).FirstOrDefault()!.ParentID,
					db.Parent.OrderBy(p => p.ParentID).FirstOrDefault()!.ParentID);
		}

		[Test]
		public void NestedFirstOrDefaultScalar1([DataSources(
			TestProvName.AllInformix, TestProvName.AllSybase, TestProvName.AllSapHana)]
			string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select    Child.FirstOrDefault()!.ChildID,
					from p in db.Parent select db.Child.FirstOrDefault()!.ChildID);
		}

		[Test]
		public void NestedFirstOrDefaultScalar2([DataSources(
			TestProvName.AllInformix, TestProvName.AllOracle,
			TestProvName.AllSybase, TestProvName.AllSapHana)]
			string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in Parent
					select new
					{
						p.ParentID,
						MaxChild =
							Child
								.Where(c => c.Parent == p)
								.OrderByDescending(c => c.ChildID * c.ParentID)
								.FirstOrDefault() == null ?
							0 :
							Child
								.Where(c => c.Parent == p)
								.OrderByDescending(c => c.ChildID * c.ParentID)
								.FirstOrDefault()!
								.ChildID
					},
					from p in db.Parent
					select new
					{
						p.ParentID,
						MaxChild = db.Child
							.Where(c => c.Parent == p)
							.OrderByDescending(c => c.ChildID * c.ParentID)
							.FirstOrDefault()!
							.ChildID
					});
		}

		[Test]
		public void NestedFirstOrDefault1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select    Child.FirstOrDefault(),
					from p in db.Parent select db.Child.FirstOrDefault());
		}

		[Test]
		public void NestedFirstOrDefault2([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select p.Children.OrderBy(c => c.ChildID).FirstOrDefault(),
					from p in db.Parent select p.Children.OrderBy(c => c.ChildID).FirstOrDefault());
		}

		[Test]
		public void NestedFirstOrDefault3([DataSources(TestProvName.AllInformix, TestProvName.AllSapHana, TestProvName.AllOracle)]
			string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select p.Children.Select(c => c.ParentID).Distinct().FirstOrDefault(),
					from p in db.Parent select p.Children.Select(c => c.ParentID).Distinct().FirstOrDefault());
		}

		[Test]
		public void NestedFirstOrDefault4([DataSources(TestProvName.AllInformix, TestProvName.AllPostgreSQL9)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select p.Children.Where(c => c.ParentID > 0).Distinct().OrderBy(_ => _.ChildID).FirstOrDefault(),
					from p in db.Parent select p.Children.Where(c => c.ParentID > 0).Distinct().OrderBy(_ => _.ChildID).FirstOrDefault());
		}

		//TODO: Access has nonstandard join, we have to improve it
		[Test]
		public void NestedFirstOrDefault5([DataSources(TestProvName.AllAccess)] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in GrandChild 
					where p.ChildID > 0
					select p.Child!.Parent!.Children.OrderBy(c => c.ChildID).FirstOrDefault(),
					from p in db.GrandChild
					where p.ChildID > 0
					select p.Child!.Parent!.Children.OrderBy(c => c.ChildID).FirstOrDefault());
		}

		[Test]
		public void NestedSingleOrDefault1([DataSources] string context)
		{
			using (var db = GetDataContext(context))
				AreEqual(
					from p in    Parent select p.Children.Select(c => c.ParentID).Distinct().SingleOrDefault(),
					from p in db.Parent select p.Children.Select(c => c.ParentID).Distinct().SingleOrDefault());
		}

		[Test]
		public void FirstOrDefaultEntitySet([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				AreEqual(
					dd.Customer.Select(c => c.Orders.FirstOrDefault()),
					db.Customer.Select(c => c.Orders.FirstOrDefault()));
			}
		}

		[Test]
		public void NestedSingleOrDefaultTest([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var dd = GetNorthwindAsList(context);
				AreEqual(
					dd.Customer.Select(c => c.Orders.Take(1).SingleOrDefault()),
					db.Customer.Select(c => c.Orders.Take(1).SingleOrDefault()));
			}
		}

		[Test]
		public void MultipleQuery([NorthwindDataContext] string context)
		{
			using (var db = new NorthwindDB(context))
			{
				var q =
					from p in db.Product
					select db.Category.Select(zrp => zrp.CategoryName).FirstOrDefault();

				var _ = q.ToList();
			}
		}
	}
}
