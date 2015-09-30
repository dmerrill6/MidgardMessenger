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


		public bool ExistsChatRoomUser(string chatroomId, string userId){
			lock (chatroomUserLocker) {
				return (chatroomUserDatabase.Table<ChatRoomUser> ().Where (x => x.chatRoomID == chatroomId && x.userID == userId).Count() > 0);
			}
		}

		public bool ExistsChatRoom(string chatroomId){
			lock (locker) {
				return (database.Table<ChatRoom> ().Where (x => x.webID == chatroomId).Count() > 0);
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

		public ChatRoomUser DeleteChatRoomUser(string userId, string chatroomId){
			lock (chatroomUserLocker) {
				ChatRoomUser cru = chatroomUserDatabase.Table<ChatRoomUser> ().FirstOrDefault (x => x.userID == userId && x.chatRoomID == chatroomId);
				chatroomUserDatabase.Delete<ChatRoomUser> (cru.ID);
				return cru;
			}
		}

		public void DeleteChatRoom(string id)
		{
			lock (locker) {
				database.Delete<ChatRoom> (id);
				return;
			}
		}
	}
}

