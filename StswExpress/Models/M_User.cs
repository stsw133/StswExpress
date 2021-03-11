using System.Collections.Generic;

namespace StswExpress.Models
{
	public class M_User
	{
		public int ID { get; set; }
		public string Username { get; set; }
		public string Newpass { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Forename { get; set; }
		public string Lastname { get; set; }
		public List<string> Perms { get; set; }

		public M_User()
		{
			Username = string.Empty;
			Newpass = string.Empty;
			Email = string.Empty;
			Phone = string.Empty;
			Forename = string.Empty;
			Lastname = string.Empty;
			Perms = new List<string>();
		}

		public string Fullname
		{
			get => $"{Lastname} {Forename}";
		}
	}
}
