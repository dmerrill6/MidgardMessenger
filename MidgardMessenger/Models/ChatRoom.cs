using System;
using SQLite;
namespace MidgardMessenger
{
	public class ChatRoom
	{
		public ChatRoom ()
		{
		}
		[PrimaryKey]
		public string webID{ get; set; }

	}
}

