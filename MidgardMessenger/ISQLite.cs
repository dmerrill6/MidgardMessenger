using System;
using SQLite;

namespace MidgardMessenger
{
	public interface ISQLite
	{
		SQLiteConnection GetConnection();
	}
}

