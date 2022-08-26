using StswExpress;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TestApp.Models;
internal class SqlConnections
{
    internal static List<DB> DBs { get; set; } = DB.LoadAllDatabases(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "databases.bin"));
}
