global using StswExpress;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TestApp;

internal class Global
{
    internal static List<StswDB> DBs { get; set; } = StswDB.LoadAllDatabases(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "databases.bin"));
}
