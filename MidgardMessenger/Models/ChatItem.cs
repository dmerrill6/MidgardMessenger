using System;
using SQLite;
namespace MidgardMessenger
{
	public class ChatItem
	{
		public ChatItem ()
		{
			
		}

		[PrimaryKey, AutoIncrement]
		public int ID {get;set;}
		public string senderID { get; set;}
		public string content { get; set; }
		public string chatroomID { get; set; }
		public string webId { get; set; }
	}
}

