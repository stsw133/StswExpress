namespace StswExpress.Globals
{
    public static class SQL
    {
		/// <summary>
		/// Set SQL connection.
		/// </summary>
		/// <param name="server">Server</param>
		/// <param name="db">Database</param>
		/// <param name="user">Username</param>
		/// <param name="pass">Password</param>
		public static string MakeConnString(string server, string db, string user, string pass) => $"Server={server};Database={db};User Id={user};Password={pass};";

		/// <summary>
		/// Set SQL connection.
		/// </summary>
		/// <param name="server">Server</param>
		/// <param name="port">Port</param>
		/// <param name="db">Database</param>
		/// <param name="user">Username</param>
		/// <param name="pass">Password</param>
		public static string MakeConnString(string server, int port, string db, string user, string pass) => $"Server={server};Port={port};Database={db};User Id={user};Password={pass};";

		/// <summary>
		/// Opens SQL connection.
		/// </summary>
		/// <param name="sqlConn">Connection to open.</param>
		/// <returns>Opened connection.</returns>
		public static object OpenConnection(object sqlConn)
		{
			((dynamic)sqlConn).Open();
			return sqlConn;
		}
	}
}
