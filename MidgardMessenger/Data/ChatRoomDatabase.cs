using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace MidgardMessenger
{
	public class ChatRoomDatabase
	{
		static object locker = new object ();
		static object chatroomUserLocker = new object ();

		SQLiteConnection database;
		SQLiteConnection chatroomUserDatabase;

		public ChatRoomDatabase ()
		{

			SQLiteMidgardMessenger sqlite = new SQLiteMidgardMessenger ();

			database = sqlite.GetConnection ();
			database.CreateTable<ChatRoom> ();

			chatroomUserDatabase = sqlite.GetConnection ();
			chatroomUserDatabase.CreateTable<ChatRoomUser> ();
		}

		public IEnumerable<ChatRoom> GetChatRooms ()
		{
			lock (locker) {
				return (from i in database.Table<ChatRoom> ()
					select i).ToList ();
			}
		}

		public IEnumerable<User> GetUsers(string chatroomId)
		{
			ChatRoom cr = GetChatRoom (chatroomId);
			List<User> result = new List<User> ();
			lock (chatroomUserLocker) {
				List<ChatRoomUser> cruList = chatroomUserDatabase.Table<ChatRoomUser> ().Where (x => x.chatRoomID == cr.webID).ToList ();
				foreach (ChatRoomUser cru in cruList) {
					User currUser = DatabaseAccessors.UserDatabaseAccessor.GetUser (cru.userID);
					result.Add (currUser);
				}
			}
			return result;

		}

		public IEnumerable<ChatRoomUser> GetChatRoomUsers(string chatroomId)
		{
			ChatRoom cr = GetChatRoom (chatroomId);
			lock (chatroomUserLocker) {
				return chatroomUserDatabase.Table<ChatRoomUser> ().Where (x => x.chatRoomID == cr.webID).ToList ();
			}


		}


		public ChatRoom GetChatRoom (string id)
		{
			lock (locker) {
				return database.Table<ChatRoom> ().FirstOrDefault (x => x.webID == id);

			}
		}
		public string SaveChatRoom (ChatRoom chatroom)
		{
			lock (locker) {
				
				database.Insert (chatroom);
				return chatroom.webID;

			}

		}


		public void SaveChatRoomUsers(List<User> users, ChatRoom chatroom)
		{
			
			lock (chatroomUserLocker) {
				foreach (User user in users) {
					ChatRoomUser cru = new ChatRoomUser ();
					cru.chatRoomID = chatroom.webID;
					cru.userID = user.webID;
					chatroomUserDatabase.Insert (cru);
				}
			}
		}


		public int DeleteChatRoom(int id)
		{
			lock (locker) {
				return database.Delete<ChatRoom> (id);
			}
		}
	}
}

