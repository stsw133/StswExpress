using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StswExpress
{
	public class M_Database
	{
		public string Name { get; set; } = string.Empty;
		public string Server { get; set; } = string.Empty;
		public int Port { get; set; } = 0;
		public string Database { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;

		/// <summary>
		/// Load list of hashed databases from file
		/// </summary>
		public static List<M_Database> LoadAllDatabases()
		{
			var result = new List<M_Database>();
			var db = new M_Database();

			try
			{
				if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases.bin")))
					File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases.bin")).Close();

				using (var stream = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases.bin")))
				{
					while (!stream.EndOfStream)
					{
						string line = stream.ReadLine();
						string property = line.Split('=')[0];

						if (property == "0")
						{
							db = new M_Database();
							db.Name =  Security.Decrypt(line.Substring(property.Length + 1));
						}
						else if (property == "1")
							db.Server = Security.Decrypt(line.Substring(property.Length + 1));
						else if (property == "2")
							db.Port = Convert.ToInt32(Security.Decrypt(line.Substring(property.Length + 1)));
						else if (property == "3")
							db.Database = Security.Decrypt(line.Substring(property.Length + 1));
						else if (property == "4")
							db.Username = Security.Decrypt(line.Substring(property.Length + 1));
						else if (property == "5")
						{
							db.Password = Security.Decrypt(line.Substring(property.Length + 1));
							result.Add(db);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{MethodBase.GetCurrentMethod().Name}{Environment.NewLine}Błąd wczytywania listy baz danych:{Environment.NewLine}{ex.Message}");
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
				using (var stream = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Databases.bin")))
				{
					foreach (M_Database db in databases)
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
			catch (Exception ex)
			{
				Console.WriteLine($"{MethodBase.GetCurrentMethod().Name}{Environment.NewLine}Błąd zapisywania listy baz danych:{Environment.NewLine}{ex.Message}");
			}
		}
	}
}
