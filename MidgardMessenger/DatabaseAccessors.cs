using System;
using Parse;

namespace MidgardMessenger
{
	public class DatabaseAccessors
	{
		static UserDatabase userDatabase;
		static ChatRoomDatabase chatroomDatabase;

		public static UserDatabase UserDatabaseAccessor{
			get{
				if (userDatabase == null) {
					userDatabase = new UserDatabase ();
				}
				return userDatabase;
			}
		}
		public static ChatRoomDatabase ChatRoomDatabaseAccessor{
			get{
				if (chatroomDatabase == null) {
					chatroomDatabase = new ChatRoomDatabase ();
				}
				return chatroomDatabase;
			}
		}
		public DatabaseAccessors ()
		{
		}

		public static User CurrentUser(){
			return UserDatabaseAccessor.GetUser (ParseUser.CurrentUser.ObjectId);
		}
	}
}

