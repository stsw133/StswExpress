﻿using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress;

/// Model for Database Connection
public class StswDB
{
    public enum Types
    {
        MSSQL,
        PostgreSQL
    }

    public string Name { get; set; } = string.Empty;
    public Types Type { get; set; } = default;
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; } = 0;
    public string Database { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    /// GetConnString
    public string? GetConnString()
    {
        return Type switch
        {
            Types.MSSQL => $"Server={Server}{(Port > 0 ? $",Port={Port}" : string.Empty)};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
            Types.PostgreSQL => $"Server={Server};Port={Port};Database={Database};User Id={Login};Password={Password};Application Name={StswFn.AppName()};",
            _ => null
        };
    }

    public static string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "databases.stsw");

    /// Load list of encrypted databases from file.
    public static List<StswDB> ImportDatabases()
    {
        var result = new List<StswDB>();

        if (!File.Exists(FilePath))
            File.Create(FilePath).Close();

        using var stream = new StreamReader(FilePath);
        while (!stream.EndOfStream)
        {
            var line = stream.ReadLine();
            if (line != null)
            {
                var data = line.Split('|');
                result.Add(new StswDB()
                {
                    Name = StswSecurity.Decrypt(data[0]),
                    Server = StswSecurity.Decrypt(data[1]),
                    Port = Convert.ToInt32(StswSecurity.Decrypt(data[2])),
                    Database = StswSecurity.Decrypt(data[3]),
                    Login = StswSecurity.Decrypt(data[4]),
                    Password = StswSecurity.Decrypt(data[5])
                });
            }
        }

        return result;
    }

    /// Save list of encrypted databases to file.
    public static void ExportDatabases(List<StswDB> databases)
    {
        using var stream = new StreamWriter(FilePath);
        foreach (var db in databases)
            stream.WriteLine(StswSecurity.Encrypt(db.Name)
                + "|" + StswSecurity.Encrypt(db.Server)
                + "|" + StswSecurity.Encrypt(db.Port.ToString())
                + "|" + StswSecurity.Encrypt(db.Database)
                + "|" + StswSecurity.Encrypt(db.Login)
                + "|" + StswSecurity.Encrypt(db.Password));
    }
}
