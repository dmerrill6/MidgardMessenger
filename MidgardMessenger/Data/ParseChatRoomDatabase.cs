using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Parse;

namespace MidgardMessenger
{
	public class ParseChatRoomDatabase
	{
		
		public ParseChatRoomDatabase ()
		{
		}

		ParseObject ToParseObject (ChatRoom chatroom)
		{
			var po = new ParseObject("ChatRoom");

			return po;
		}

		ParseObject ToParseObject(ChatRoomUser cru)
		{
			var po = new ParseObject ("ChatRoomUser");
			po ["userId"] = cru.userID;
			po ["chatroomId"] = cru.chatRoomID;
			return po;
		}

		static ChatRoom FromParseObject (ParseObject po)
		{
			var t = new ChatRoom ();
			t.webID = po.ObjectId;
			return t;
		}

		public async Task SaveChatRoomAsync(ChatRoom chatroom)
		{
			ParseObject po = ToParseObject (chatroom);
			await po.SaveAsync ();
			chatroom.webID = po.ObjectId;
		}

		public async Task SaveChatRoomUsersAsync(ChatRoomUser cru){
			ParseObject po = ToParseObject (cru);
			await po.SaveAsync ();
		}
			
	}
}

