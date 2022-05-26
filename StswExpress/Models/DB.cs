using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StswExpress
{
    /// Model for Database Connection
    public class DB
    {
        public string Name { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        /// GetConnString
        public string GetMssqlConnString() => $"Server={Server},{Port};Database={Database};User Id={Username};Password={Password};";
        public string GetNpgsqlConnString() => $"Server={Server};Port={Port};Database={Database};User Id={Username};Password={Password};";

        /// Load list of encrypted databases from file.
        public static List<DB> LoadAllDatabases(string path)
        {
            var result = new List<DB>();
            var db = new DB();

            if (!File.Exists(path))
                File.Create(path).Close();

            using var stream = new StreamReader(path);
            while (!stream.EndOfStream)
            {
                var line = stream.ReadLine();
                if (line != null)
                {
                    var property = line.Split('=')?[0];
                    switch (property)
                    {
                        case "0":
                            db = new DB();
                            db.Name = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "1":
                            db.Server = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "2":
                            db.Port = Convert.ToInt32(Security.Decrypt(line[(property.Length + 1)..]));
                            break;
                        case "3":
                            db.Database = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "4":
                            db.Username = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "5":
                            db.Password = Security.Decrypt(line[(property.Length + 1)..]);
                            result.Add(db);
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        /// Save list of encrypted databases to file.
        public static void SaveAllDatabases(List<DB> databases, string path)
        {
            using var stream = new StreamWriter(path);
            foreach (var db in databases)
            {
                stream.WriteLine($"0={Security.Encrypt(db.Name)}");
                stream.WriteLine($"1={Security.Encrypt(db.Server)}");
                stream.WriteLine($"2={Security.Encrypt(db.Port.ToString())}");
                stream.WriteLine($"3={Security.Encrypt(db.Database)}");
                stream.WriteLine($"4={Security.Encrypt(db.Username)}");
                stream.WriteLine($"5={Security.Encrypt(db.Password)}");
                stream.WriteLine(string.Empty);
            }
        }
    }
}
