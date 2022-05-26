using System;
using System.Collections.Generic;
using System.IO;

namespace StswExpress
{
    /// Model for Mail Configuration
    public class MC
    {
        public string Name { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public string Address { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSSL { get; set; } = false;

        /// Load list of encrypted mail configs from file.
        public static List<MC> LoadAllMailConfigs(string path)
        {
            var result = new List<MC>();
            var mc = new MC();

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
                            mc = new MC();
                            mc.Name = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "1":
                            mc.Host = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "2":
                            mc.Port = Convert.ToInt32(Security.Decrypt(line[(property.Length + 1)..]));
                            break;
                        case "3":
                            mc.Address = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "4":
                            mc.Username = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "5":
                            mc.Password = Security.Decrypt(line[(property.Length + 1)..]);
                            break;
                        case "6":
                            mc.EnableSSL = Convert.ToBoolean(Security.Decrypt(line[(property.Length + 1)..]));
                            result.Add(mc);
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        /// Save list of encrypted mail configs to file.
        public static void SaveAllMailConfigs(List<MC> mailConfigs, string path)
        {
            using var stream = new StreamWriter(path);
            foreach (var mc in mailConfigs)
            {
                stream.WriteLine($"0={Security.Encrypt(mc.Name)}");
                stream.WriteLine($"1={Security.Encrypt(mc.Host)}");
                stream.WriteLine($"2={Security.Encrypt(mc.Port.ToString())}");
                stream.WriteLine($"3={Security.Encrypt(mc.Address)}");
                stream.WriteLine($"4={Security.Encrypt(mc.Username)}");
                stream.WriteLine($"5={Security.Encrypt(mc.Password)}");
                stream.WriteLine($"6={Security.Encrypt(mc.EnableSSL.ToString())}");
                stream.WriteLine(string.Empty);
            }
        }
    }
}
