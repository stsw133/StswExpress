namespace StswExpress
{
    public static class SQL
    {
        /// MakeConnString
        public static string MakeConnString(string server, string db, string user, string pass) => $"Server={server};Database={db};User Id={user};Password={pass};";
        public static string MakeConnString(string server, int port, string db, string user, string pass) => $"Server={server};Port={port};Database={db};User Id={user};Password={pass};";
    }
}
