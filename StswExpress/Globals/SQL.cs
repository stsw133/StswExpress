namespace StswExpress.Globals
{
    public static class SQL
    {
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
	}
}
