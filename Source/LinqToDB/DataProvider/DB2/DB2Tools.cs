﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using JetBrains.Annotations;

namespace LinqToDB.DataProvider.DB2
{
	using System.Data.Common;
	using Configuration;
	using Data;

	[PublicAPI]
	public static class DB2Tools
	{
		static readonly Lazy<IDataProvider> _db2DataProviderzOS = DataConnection.CreateDataProvider<DB2zOSDataProvider>();
		static readonly Lazy<IDataProvider> _db2DataProviderLUW = DataConnection.CreateDataProvider<DB2LUWDataProvider>();

		public static bool AutoDetectProvider { get; set; } = true;

		internal static IDataProvider? ProviderDetector(IConnectionStringSettings css, string connectionString)
		{
			// DB2 ODS provider could be used by informix
			if (css.Name.Contains("Informix"))
				return null;

			switch (css.ProviderName)
			{
				case ProviderName.DB2LUW: return _db2DataProviderLUW.Value;
				case ProviderName.DB2zOS: return _db2DataProviderzOS.Value;

				case ""             :
				case null           :

					if (css.Name == "DB2")
						goto case ProviderName.DB2;
					break;

				case ProviderName.DB2    :
				case DB2ProviderAdapter.NetFxClientNamespace:
				case DB2ProviderAdapter.CoreClientNamespace :

					if (css.Name.Contains("LUW"))
						return _db2DataProviderLUW.Value;
					if (css.Name.Contains("z/OS") || css.Name.Contains("zOS"))
						return _db2DataProviderzOS.Value;

					if (AutoDetectProvider)
					{
						try
						{
							var cs = string.IsNullOrWhiteSpace(connectionString) ? css.ConnectionString : connectionString;

							using (var conn = DB2ProviderAdapter.Instance.CreateConnection(cs))
							{
								conn.Open();

								var iszOS = conn.eServerType == DB2ProviderAdapter.DB2ServerTypes.DB2_390;

								return iszOS ? _db2DataProviderzOS.Value : _db2DataProviderLUW.Value;
							}
						}
						catch
						{
						}
					}

					return GetDataProvider();
			}

			return null;
		}

		public static IDataProvider GetDataProvider(DB2Version version = DB2Version.LUW)
		{
			if (version == DB2Version.zOS)
				return _db2DataProviderzOS.Value;

			return _db2DataProviderLUW.Value;
		}

		public static void ResolveDB2(string path)
		{
			new AssemblyResolver(path, DB2ProviderAdapter.AssemblyName);
			if (DB2ProviderAdapter.AssemblyNameOld != null)
#pragma warning disable CS0162 // Unreachable code detected
				new AssemblyResolver(path, DB2ProviderAdapter.AssemblyNameOld);
#pragma warning restore CS0162 // Unreachable code detected
		}

		public static void ResolveDB2(Assembly assembly)
		{
			new AssemblyResolver(assembly, assembly.GetName().Name!);
		}

		#region CreateDataConnection

		/// <summary>
		/// Creates <see cref="DataConnection"/> object using provided DB2 connection string.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		/// <param name="version">DB2 version.</param>
		/// <returns><see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateDataConnection(string connectionString, DB2Version version = DB2Version.LUW)
		{
			return new DataConnection(GetDataProvider(version), connectionString);
		}

		/// <summary>
		/// Creates <see cref="DataConnection"/> object using provided connection object.
		/// </summary>
		/// <param name="connection">Connection instance.</param>
		/// <param name="version">DB2 version.</param>
		/// <returns><see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateDataConnection(DbConnection connection, DB2Version version = DB2Version.LUW)
		{
			return new DataConnection(GetDataProvider(version), connection);
		}

		/// <summary>
		/// Creates <see cref="DataConnection"/> object using provided transaction object.
		/// </summary>
		/// <param name="transaction">Transaction instance.</param>
		/// <param name="version">DB2 version.</param>
		/// <returns><see cref="DataConnection"/> instance.</returns>
		public static DataConnection CreateDataConnection(DbTransaction transaction, DB2Version version = DB2Version.LUW)
		{
			return new DataConnection(GetDataProvider(version), transaction);
		}

		#endregion

		#region BulkCopy

		/// <summary>
		/// Default bulk copy mode, used for DB2 by <see cref="DataConnectionExtensions.BulkCopy{T}(DataConnection, IEnumerable{T})"/>
		/// methods, if mode is not specified explicitly.
		/// Default value: <see cref="BulkCopyType.MultipleRows"/>.
		/// </summary>
		public static BulkCopyType  DefaultBulkCopyType { get; set; } = BulkCopyType.MultipleRows;

		#endregion
	}
}
