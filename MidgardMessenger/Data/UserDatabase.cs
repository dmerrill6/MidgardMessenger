using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;


namespace MidgardMessenger
{
	public class UserDatabase
	{

		static object locker = new object ();
		SQLiteConnection database;

		public UserDatabase ()
		{

			SQLiteMidgardMessenger sqlite = new SQLiteMidgardMessenger ();

			database = sqlite.GetConnection ();
			database.CreateTable<User> ();
		}

		public IEnumerable<User> GetUsers ()
		{
			lock (locker) {
				return (from i in database.Table<User> ()
					select i).ToList ();
			}
		}

		public User GetUser (string webId)
		{
			lock (locker) {
				return database.Table<User> ().FirstOrDefault (x => x.webID == webId);

			}
		}
		public int SaveUser (User user)
		{
			lock (locker) {
				if (user.ID != 0) {
					database.Update (user);
					return user.ID;
				} else {
					return database.Insert (user);
				}
			}

		}

		public int SaveUser(string fullName, string webUserId)
		{
			User user = null;

			lock (locker) {
				user = database.Table<User> ().FirstOrDefault (x => x.webID == webUserId);
				if (user != null)
					user.name = fullName;
				else {
					user = new User ();
					user.name = fullName;
					user.webID = webUserId;
				}
			}
			return SaveUser (user);
		}

		public int DeleteUser(int id)
		{
			lock (locker) {
				return database.Delete<User> (id);
			}
		}
	}
}

