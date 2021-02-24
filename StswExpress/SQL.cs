using MySqlConnector;
using Npgsql;
using System.Data.SqlClient;

namespace StswExpress
{
    public static class SQL
    {
		public enum Type
        {
			MSSQL, MySQL, Postgres
        }
		public static Type SqlType = Type.MSSQL;

		/// <summary>
		/// Default connection string
		/// </summary>
		public static string connDef = null;
		public static SqlConnection opconnDef => (SqlConnection)OpenConnection(new SqlConnection(connDef));
		public static MySqlConnection opmyconnDef => (MySqlConnection)OpenConnection(new MySqlConnection(connDef));
		public static NpgsqlConnection opnpgconnDef => (NpgsqlConnection)OpenConnection(new NpgsqlConnection(connDef));

		/// <summary>
		/// Set SQL connection.
		/// </summary>
		/// <param name="server">Server</param>
		/// <param name="port">Port</param>
		/// <param name="database">Database</param>
		/// <param name="username">Username</param>
		/// <param name="password">Password</param>
		public static string MakeConnString(string server, int port, string database, string username, string password)
		{
			return $"Server={server};Port={port};Database={database};User Id={username};Password={password};";
		}

		/// <summary>
		/// Opens received SQL connection and returns it.
		/// </summary>
		/// <param name="sqlConn">Connection to open.</param>
		/// <returns>Opened connection.</returns>
		public static object OpenConnection(object sqlConn)
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
