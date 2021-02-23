using MySqlConnector;
using Npgsql;
using System.Data.SqlClient;

namespace StswExpress
{
    internal static class SQL
    {
		internal enum Type
        {
			MSSQL, MySQL, Postgres
        }
		internal static Type SqlType = Type.MSSQL;

		/// <summary>
		/// Default connection string
		/// </summary>
		internal static string connDef = null;
		internal static SqlConnection opconnDef => (SqlConnection)OpenConnection(new SqlConnection(connDef));
		internal static MySqlConnection opmyconnDef => (MySqlConnection)OpenConnection(new MySqlConnection(connDef));
		internal static NpgsqlConnection opnpgconnDef => (NpgsqlConnection)OpenConnection(new NpgsqlConnection(connDef));

		/// <summary>
		/// Set SQL connection.
		/// </summary>
		/// <param name="server">Server</param>
		/// <param name="port">Port</param>
		/// <param name="database">Database</param>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		internal static string MakeConnString(string server, int port, string database, string username, string password)
		{
			return $"Server={server};Port={port};Database={database};User Id={username};Password={password};";
		}

		/// <summary>
		/// Opens received SQL connection and returns it.
		/// </summary>
		/// <param name="sqlConn">Connection to open.</param>
		/// <returns>Opened connection.</returns>
		internal static object OpenConnection(object sqlConn)
		{
			if (SqlType == Type.MSSQL)
				(sqlConn as SqlConnection).Open();
			else if (SqlType == Type.MySQL)
				(sqlConn as MySqlConnection).Open();
			else if (SqlType == Type.Postgres)
				(sqlConn as NpgsqlConnection).Open();
			return sqlConn;
		}
	}
}
