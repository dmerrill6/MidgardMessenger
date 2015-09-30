using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Parse;

namespace MidgardMessenger
{
	public class ParseChatItemDatabase
	{
		public ParseChatItemDatabase ()
		{
		}


		ParseObject ToParseObject (ChatItem chatitem)
		{
			var po = new ParseObject("Chat");
			po ["chatroomId"] = chatitem.chatroomID;
			po ["userId"] = chatitem.senderID;
			po ["content"] = chatitem.content;
			return po;
		}


		static ChatItem FromParseObject (ParseObject po)
		{
			var t = new ChatItem ();
			t.webId = po.ObjectId;
			t.chatroomID = Convert.ToString(po ["chatroomId"]);
			var test = po ["content"];
			if (po ["content"] != null)
				t.content = Convert.ToString (po ["content"]);
			else
				t.content = "";	
			t.senderID = Convert.ToString(po ["userId"]);
			return t;
		}

		public async Task SaveChatItemAsync(ChatItem chatItem)
		{
			ParseObject po = ToParseObject (chatItem);
			await po.SaveAsync ();
			chatItem.webId = po.ObjectId;
		}

		public async Task GetAndSyncChatItemsAsync(string chatroomId){
			var query = ParseObject.GetQuery ("Chat").WhereEqualTo ("chatroomId", chatroomId);
			var results = await query.FindAsync ();

			foreach (ParseObject chatPO in results) {
				ChatItem chat = FromParseObject (chatPO);
				if (DatabaseAccessors.ChatDatabaseAccessor.ExistsChat (chat.webId) == false)
					DatabaseAccessors.ChatDatabaseAccessor.SaveItem (chat);
				
			}
		}
	}
}

