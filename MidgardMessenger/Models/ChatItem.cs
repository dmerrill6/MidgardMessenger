using System;
using SQLite;
namespace MidgardMessenger
{
	public class ChatItem
	{
		public ChatItem ()
		{
			read = false;
			createdAt = DateTime.Now;
		}

		[PrimaryKey, AutoIncrement]
		public int ID {get;set;}
		public string senderID { get; set;}
		public string content { get; set; }
		public string chatroomID { get; set; }
		public string webId { get; set; }
		public string pathToFile {get; set; }
		public string fileName { get; set; }
		public bool read { get; set; }
		public DateTime createdAt { get ; set; }
		public string extra { get; set; }
		public string extra2 { get; set; }
	}
}

