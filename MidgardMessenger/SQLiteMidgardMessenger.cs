using System.IO;
using System;
using Xamarin.Forms;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using Android.Database;

namespace MidgardMessenger
{
	public class SQLiteMidgardMessenger : ISQLite
	{
		public SQLiteMidgardMessenger ()
		{
		}

		public SQLite.SQLiteConnection GetConnection()
		{
			var sqliteFilename = "MidgardMessengerSQLite.db3";
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			var path = Path.Combine (documentsPath, sqliteFilename);

			if (!File.Exists (path)) {
				

				var s = Forms.Context.Resources.OpenRawResource(Resource.Raw.MidgardMessengerSQLite);

				FileStream writeStream = new FileStream (path, FileMode.OpenOrCreate, FileAccess.Write);

				ReadWriteStream (s, writeStream);
			}

			var conn = new SQLite.SQLiteConnection (path);

			return conn;
		}

		void ReadWriteStream(Stream readStream, Stream writeStream)
		{
			int Length = 256;
			Byte[] buffer = new Byte[Length];
			int bytesRead = readStream.Read(buffer, 0, Length);
			// write the required bytes
			while (bytesRead > 0)
			{
				writeStream.Write(buffer, 0, bytesRead);
				bytesRead = readStream.Read(buffer, 0, Length);
			}
			readStream.Close();
			writeStream.Close();
		}
	}
}

