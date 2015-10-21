using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MidgardMessenger
{
	public class ChatItemDatabase
	{
		static object locker = new object ();
		SQLiteConnection database;

		public ChatItemDatabase ()
		{
			SQLiteMidgardMessenger sqlite = new SQLiteMidgardMessenger ();
			database = sqlite.GetConnection ();
			database.CreateTable<ChatItem> ();
		}

		public bool ExistsChat(string chatId){
			lock (locker) {
				return (database.Table<ChatItem> ().Where (x => x.webId == chatId).Count () > 0);
			}
		}

		public IEnumerable<ChatItem> GetChats ()
		{
			lock (locker) {
				return (from i in database.Table<ChatItem> ()
				        select i).ToList ();
			}
		}

		public IEnumerable<ChatItem> GetChats (string chatroomId)
		{
			lock (locker) {
				return (from i in database.Table<ChatItem> ().Where(x => x.chatroomID == chatroomId)
					select i).ToList ();
			}
		}

		public void MarkAsRead (ChatRoom chatroom)
		{
			var chats = GetChats (chatroom.webID);
			foreach (ChatItem chat in chats) {
				chat.read = true;
				SaveItem(chat);
			}
		}
		public int GetUnread (ChatRoom chatroom)
		{
			lock (locker) {
				return( from i in database.Table<ChatItem>().Where(x => x.chatroomID == chatroom.webID).Where(x => x.read == false)
					select i).ToList().Count;
			}
		}
		public ChatItem GetItem (int id)
		{
			lock (locker) {
				return database.Table<ChatItem> ().FirstOrDefault (x => x.ID == id);

			}
		}

		public ChatItem GetItem (string webId)
		{
			lock (locker) {
				return database.Table<ChatItem> ().FirstOrDefault (x => x.webId == webId);

			}
		}

		public int SaveItem (ChatItem item)
		{
			lock (locker) {
				if (item.ID != 0) {
					database.Update (item);
					return item.ID;
				} else {
					return database.Insert (item);
				}
			}

		}

		public int DeleteItem(int id)
		{
			lock (locker) {
				return database.Delete<ChatItem> (id);
			}
		}
	}
}

