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

		public IEnumerable<ChatItem> GetChats ()
		{
			lock (locker) {
				return (from i in database.Table<ChatItem> ()
				        select i).ToList ();
			}
		}

		public ChatItem GetItem (int id)
		{
			lock (locker) {
				return database.Table<ChatItem> ().FirstOrDefault (x => x.ID == id);

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

