using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Parse;
using System.IO;
using System.Net;
using Java.IO;
using Android.Widget;

namespace MidgardMessenger
{
	public class ParseChatItemDatabase
	{
		public ParseChatItemDatabase ()
		{
		}


		ParseObject ToParseObject (ChatItem chatitem)
		{
			var po = new ParseObject ("Chat");
			po ["chatroomId"] = chatitem.chatroomID;
			po ["userId"] = chatitem.senderID;
			po ["content"] = chatitem.content;
			if (chatitem.fileName != null && chatitem.pathToFile != null) {
				byte[] data = System.IO.File.ReadAllBytes(chatitem.pathToFile + "/" + chatitem.fileName);
				ParseFile file = new ParseFile(chatitem.fileName, data);
				po["fileData"] = file;
			}
			return po;
		}


		static ChatItem FromParseObject (ParseObject po)
		{
			var t = new ChatItem ();
			t.webId = po.ObjectId;
			t.chatroomID = Convert.ToString (po ["chatroomId"]);
			var test = po ["content"];
			if (po ["content"] != null)
				t.content = Convert.ToString (po ["content"]);
			else
				t.content = "";	
			t.senderID = Convert.ToString (po ["userId"]);
			if (po.Keys.Contains ("fileData") && po ["fileData"] != null) {
				ParseFile pf = (ParseFile)po ["fileData"];
				t.pathToFile = UtilsAndConstants.ImagesDir.Path;
				t.fileName = pf.Name;
				if (pf.Name.IndexOf ("MidgardPhoto") == -1) {
					UtilsAndConstants.DownloadRemoteImageFile (pf.Url, t.pathToFile + "/" + t.fileName);
					
				} else {
					var file = new Java.IO.File (Android.OS.Environment.GetExternalStoragePublicDirectory (Android.OS.Environment.DirectoryPictures) + "/" + t.fileName.Substring (t.fileName.IndexOf ("MidgardPhoto")));
					if (!file.Exists ()) {
						
						UtilsAndConstants.DownloadRemoteImageFile (pf.Url, file.Path);
						

					} else 
					{
						t.fileName = t.fileName.Substring (t.fileName.IndexOf ("MidgardPhoto"));
					}
				}

			}
			return t;
		}

		public async Task SaveChatItemAsync(ChatItem chatItem)
		{
			ParseObject po = ToParseObject (chatItem);

			chatItem.webId = po.ObjectId;
		}

		public async Task SaveChatItemAsync(ChatItem chatItem, Action<int,string> actionCallback)
		{
			ParseObject po = ToParseObject (chatItem);
			ParseFile pf = (ParseFile)po["fileData"];
			await pf.SaveAsync (new Progress<ParseUploadProgressEventArgs>(e => {
				System.Console.WriteLine(e.Progress + "%");
				System.Threading.Thread.Sleep(1000);
			}));
			await po.SaveAsync();
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

