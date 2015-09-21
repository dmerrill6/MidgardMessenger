using System;
using SQLite;
namespace MidgardMessenger
{
	public class User
	{
		public User ()
		{
			
		}

		[PrimaryKey, AutoIncrement]
		public int ID {get;set;}
		public string name {get; set; }
		public string webID { get; set; }
	}
}

