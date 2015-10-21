using System;
using SQLite;
namespace MidgardMessenger
{
	public class ChatRoom
	{
		public ChatRoom ()
		{
			createdAt = DateTime.Now;
		}

		[PrimaryKey]
		public string webID{ get; set; }
		public string chatRoomName {get; set;}
		public DateTime createdAt {get;set;}
	
	}
}

