using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress
{
    /// Model for Database Connection
    public class DB
    {
        public string Name { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Database { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        /// GetConnString
        public string GetMssqlConnString() => $"Server={Server}{(Port > 0 ? $",Port={Port}" : string.Empty)};Database={Database};User Id={Login};Password={Password};Application Name={Fn.AppName()};";
        public string GetNpgsqlConnString() => $"Server={Server};Port={Port};Database={Database};User Id={Login};Password={Password};Application Name={Fn.AppName()};";

        /// Load list of encrypted databases from file.
        public static List<DB> LoadAllDatabases(string path)
        {
            var result = new List<DB>();

            if (!File.Exists(path))
                File.Create(path).Close();

            using var stream = new StreamReader(path);
            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine();
                if (line != null)
                {
                    var data = line.Split('|');
                    result.Add(new DB()
                    {
                        Name = Security.Decrypt(data[0]),
                        Server = Security.Decrypt(data[1]),
                        Port = Convert.ToInt32(Security.Decrypt(data[2])),
                        Database = Security.Decrypt(data[3]),
                        Login = Security.Decrypt(data[4]),
                        Password = Security.Decrypt(data[5])
                    });
                }
            }

            return result;
        }

        /// Save list of encrypted databases to file.
        public static void SaveAllDatabases(List<DB> databases, string path)
        {
            using var stream = new StreamWriter(path);
            foreach (var db in databases)
                stream.WriteLine(Security.Encrypt(db.Name)
                    + "|" + Security.Encrypt(db.Server)
                    + "|" + Security.Encrypt(db.Port.ToString())
                    + "|" + Security.Encrypt(db.Database)
                    + "|" + Security.Encrypt(db.Login)
                    + "|" + Security.Encrypt(db.Password));
        }
    }
}
