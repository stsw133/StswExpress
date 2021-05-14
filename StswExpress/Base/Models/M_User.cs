using System.Collections.Generic;

namespace StswExpress
{
	public class M_User
	{
		public int ID { get; set; } = 0;
		public string Username { get; set; } = string.Empty;
		public string Newpass { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Forename { get; set; } = string.Empty;
		public string Lastname { get; set; } = string.Empty;
		public List<string> Perms { get; set; } = new List<string>();

		public string Fullname => $"{Lastname} {Forename}";
	}
}
