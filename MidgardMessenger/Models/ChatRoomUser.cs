
using System;
using SQLite;
namespace MidgardMessenger
{
	public class ChatRoomUser
	{
		public ChatRoomUser ()
		{
		}
		[PrimaryKey, AutoIncrement]
		public int ID {get;set;}
		public string userID {get; set;}
		public string chatRoomID {get; set;}
	}
}

