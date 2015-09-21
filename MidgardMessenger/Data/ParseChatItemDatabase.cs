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
			return po;
		}


		static ChatItem FromParseObject (ParseObject po)
		{
			var t = new ChatItem ();
			t.webId = po.ObjectId;
			t.chatroomID = Convert.ToString(po ["chatroomId"]);
			t.content = Convert.ToString(po ["content"]);
				t.senderID = Convert.ToString(po ["userId"]);
			return t;
		}

		public async Task SaveChatItemAsync(ChatItem chatItem)
		{
			ParseObject po = ToParseObject (chatItem);
			await po.SaveAsync ();
			chatItem.webId = po.ObjectId;
		}
	}
}

