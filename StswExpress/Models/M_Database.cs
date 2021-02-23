using StswExpress.Globals;
using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress.Models
{
	public class M_Database
	{
		public string Name { get; set; }
		public string Server { get; set; }
		public int Port { get; set; }
		public string Database { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Version { get; set; }

		public M_Database()
		{
			Name = string.Empty;
			Server = string.Empty;
			Database = string.Empty;
			Username = string.Empty;
			Password = string.Empty;
			Version = string.Empty;
		}

		/// <summary>
		/// Load list of hashed databases from file
		/// </summary>
		public static List<M_Database> LoadAllDatabases()
		{
			var result = new List<M_Database>();
			var db = new M_Database();

			try
			{
				if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Databases.bin")))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
					File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Databases.bin")).Close();
				}

				using (var stream = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Databases.bin")))
				{
					while (!stream.EndOfStream)
					{
						string line = stream.ReadLine();
						string property = line.Split('=')[0];

						if (property == "Name")
						{
							db = new M_Database();
							db.Name =  Global.Decrypt(line.Substring(property.Length + 1));
						}
						else if (property == "Server")
							db.Server = Global.Decrypt(line.Substring(property.Length + 1));
						else if (property == "Port")
							db.Port = Convert.ToInt32(Global.Decrypt(line.Substring(property.Length + 1)));
						else if (property == "Database")
							db.Database = Global.Decrypt(line.Substring(property.Length + 1));
						else if (property == "Username")
							db.Username = Global.Decrypt(line.Substring(property.Length + 1));
						else if (property == "Password")
						{
							db.Password = Global.Decrypt(line.Substring(property.Length + 1));
							result.Add(db);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Błąd wczytywania listy baz danych: {ex.Message}");
			}

			return result;
		}

		/// <summary>
		/// Save list of hashed databases to file
		/// </summary>
		public static void SaveAllDatabases(List<M_Database> databases)
		{
			try
			{
				using (var stream = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\Databases.bin")))
				{
					foreach (M_Database db in databases)
					{
						stream.WriteLine($"Name={Global.Encrypt(db.Name)}");
						stream.WriteLine($"Server={Global.Encrypt(db.Server)}");
						stream.WriteLine($"Port={Global.Encrypt(db.Port.ToString())}");
						stream.WriteLine($"Database={Global.Encrypt(db.Database)}");
						stream.WriteLine($"Username={Global.Encrypt(db.Username)}");
						stream.WriteLine($"Password={Global.Encrypt(db.Password)}");
						stream.WriteLine(string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Błąd zapisywania listy baz danych: {ex.Message}");
			}
		}
	}
}
