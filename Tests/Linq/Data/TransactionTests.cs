﻿using System.Linq;

using LinqToDB;

using NUnit.Framework;

namespace Tests.Data
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Model;

	[TestFixture]
	public class TransactionTests : TestBase
	{
		[Test]
		public async Task DataContextBeginTransactionAsync([DataSources(false)] string context)
		{
			using (var db = new DataContext(context))
			{
				// ensure connection opened and test results not affected by OpenAsync
				db.KeepConnectionAlive = true;
				await db.GetTable<Parent>().ToListAsync();

				var tid = Environment.CurrentManagedThreadId;

				using (await db.BeginTransactionAsync())
				{
					// perform synchonously to not mess with BeginTransactionAsync testing
					db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

					if (tid == Environment.CurrentManagedThreadId)
						Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
				}
			}
		}

		[Test]
		public async Task DataContextOpenOrBeginTransactionAsync([DataSources(false)] string context)
		{
			var tid = Environment.CurrentManagedThreadId;

			using (var db = new DataContext(context))
			using (await db.BeginTransactionAsync())
			{
				// perform synchonously to not mess with BeginTransactionAsync testing
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public async Task DataContextCommitTransactionAsync([DataSources(false)] string context)
		{
			using (var db = new DataContext(context))
			using (var tr = await db.BeginTransactionAsync())
			{
				int tid;
				try
				{
					await db.InsertAsync(new Parent { ParentID = 1010, Value1 = 1010 });

					tid = Environment.CurrentManagedThreadId;

					await tr.CommitTransactionAsync();
				}
				finally
				{
					// perform synchronously to not mess with CommitTransactionAsync testing
					db.GetTable<Parent>().Where(_ => _.ParentID == 1010).Delete();
				}

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public async Task DataContextRollbackTransactionAsync([DataSources(false)] string context)
		{
			using (var db = new DataContext(context))
			using (var tr = await db.BeginTransactionAsync())
			{
				await db.InsertAsync(new Parent { ParentID = 1010, Value1 = 1010 });

				var tid = Environment.CurrentManagedThreadId;

				await tr.RollbackTransactionAsync();

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public async Task DataConnectionBeginTransactionAsync([DataSources(false)] string context)
		{
			var tid = Environment.CurrentManagedThreadId;

			using (var db = GetDataConnection(context))
			using (await db.BeginTransactionAsync())
			{
				// perform synchonously to not mess with BeginTransactionAsync testing
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public async Task DataConnectionDisposeAsyncTransaction([DataSources(false)] string context)
		{
			var tid = Environment.CurrentManagedThreadId;

			using (var db = GetDataConnection(context))
			{
				await using (db.BeginTransaction())
				{
					// perform synchonously to not mess with DisposeAsync testing
					db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

					if (tid == Environment.CurrentManagedThreadId)
						Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
				}
			}
		}

		[Test]
		public async Task DataConnectionCommitTransactionAsync([DataSources(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (await db.BeginTransactionAsync())
			{
				int tid;
				try
				{
					await db.InsertAsync(new Parent { ParentID = 1010, Value1 = 1010 });

					tid = Environment.CurrentManagedThreadId;

					await db.CommitTransactionAsync();
				}
				finally
				{
					// perform synchonously to not mess with CommitTransactionAsync testing
					db.GetTable<Parent>().Where(_ => _.ParentID == 1010).Delete();
				}

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public async Task DataConnectionRollbackTransactionAsync([DataSources(false)] string context)
		{
			using (var db = GetDataConnection(context))
			using (await db.BeginTransactionAsync())
			{
				// perform synchonously to not mess with BeginTransactionAsync testing
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				var tid = Environment.CurrentManagedThreadId;

				await db.RollbackTransactionAsync();

				if (tid == Environment.CurrentManagedThreadId)
					Assert.Inconclusive("Executed synchronously due to lack of async support or there were no underlying async operations");
			}
		}

		[Test]
		public void AutoRollbackTransaction([DataSources(false)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				try
				{
					using (db.BeginTransaction())
					{
						db.Parent.Update(t => t.ParentID == 1010, t => new Parent { Value1 = 1012 });
					}

					var p = db.Parent.First(t => t.ParentID == 1010);

					Assert.That(p.Value1, Is.Not.EqualTo(1012));
				}
				finally
				{
					db.Parent.Delete(t => t.ParentID >= 1000);
				}
			}
		}

		[Test]
		public void CommitTransaction([DataSources(false)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				try
				{
					using (var tr = db.BeginTransaction())
					{
						db.Parent.Update(t => t.ParentID == 1010, t => new Parent { Value1 = 1011 });
						tr.Commit();
					}

					var p = db.Parent.First(t => t.ParentID == 1010);

					Assert.That(p.Value1, Is.EqualTo(1011));
				}
				finally
				{
					db.Parent.Delete(t => t.ParentID >= 1000);
				}
			}
		}

		[Test]
		public void RollbackTransaction([DataSources(false)] string context)
		{
			using (var db = GetDataConnection(context))
			{
				db.Insert(new Parent { ParentID = 1010, Value1 = 1010 });

				try
				{
					using (var tr = db.BeginTransaction())
					{
						db.Parent.Update(t => t.ParentID == 1010, t => new Parent {Value1 = 1012});
						tr.Rollback();
					}

					var p = db.Parent.First(t => t.ParentID == 1010);

					Assert.That(p.Value1, Is.Not.EqualTo(1012));
				}
				finally
				{
					db.Parent.Delete(t => t.ParentID >= 1000);
				}
			}
		}
	}
}
