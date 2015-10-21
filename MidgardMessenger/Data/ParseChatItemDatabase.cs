using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Parse;
using System.IO;
using System.Net;
using Java.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
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
				byte[] data = System.IO.File.ReadAllBytes (chatitem.pathToFile + "/" + chatitem.fileName);
				ParseFile file = new ParseFile (chatitem.fileName, data);
				po ["fileData"] = file;
			}
			if (chatitem.extra != null && chatitem.extra2 != null) {
				byte[] data = System.IO.File.ReadAllBytes (chatitem.extra2 + "/" + chatitem.extra);
				ParseFile file = new ParseFile (chatitem.extra, data);
				po ["extraFile"] = file;
			}
			return po;
		}


		static ChatItem FromParseObject (ParseObject po)
		{
			var t = new ChatItem ();
			t.webId = po.ObjectId;
			t.chatroomID = Convert.ToString (po ["chatroomId"]);
			t.createdAt = (po.CreatedAt.Value);

			if (po ["content"] != null)
				t.content = Convert.ToString (po ["content"]);
			
			else
				t.content = "";	
			t.senderID = Convert.ToString (po ["userId"]);
			if (po.Keys.Contains ("fileData") && po ["fileData"] != null) {
				ParseFile pf = (ParseFile)po ["fileData"];
				t.pathToFile = UtilsAndConstants.ImagesDir.Path;
				t.fileName = pf.Name;
				if (pf.Name.IndexOf ("MidgardPhoto") != -1) {
					string actualFileName = t.fileName.Substring (t.fileName.IndexOf ("MidgardPhoto"));
					var file = new Java.IO.File (UtilsAndConstants.ImagesDir + "/" + actualFileName);
					if (!file.Exists ()) {
						UtilsAndConstants.DownloadRemoteImageFile (pf.Url, actualFileName);
					}
					t.fileName = actualFileName;

				} else {
					string actualFileName = t.fileName;
					var file = new Java.IO.File (UtilsAndConstants.ImagesDir + "/" + actualFileName);
					if (!file.Exists ()) {
						UtilsAndConstants.DownloadRemoteImageFile (pf.Url, actualFileName);
					}

				}
			}
			if (po.Keys.Contains ("extraFile") && po ["extraFile"] != null) {
				ParseFile pf = (ParseFile)po ["extraFile"];
				t.extra2 = UtilsAndConstants.ImagesDir.Path;
				t.extra = pf.Name;
				if (pf.Name.IndexOf ("MidgardPhoto") != -1) {
					string actualFileName = t.extra.Substring (t.extra.IndexOf ("MidgardPhoto"));
					var file = new Java.IO.File (UtilsAndConstants.ImagesDir + "/" + actualFileName);
					if (!file.Exists ()) {
						UtilsAndConstants.DownloadRemoteImageFile (pf.Url, actualFileName);
					}
					t.extra = actualFileName;

				} else {
					string actualFileName = t.extra;
					var file = new Java.IO.File (UtilsAndConstants.ImagesDir + "/" + actualFileName);
					if (!file.Exists ()) {
						UtilsAndConstants.DownloadRemoteImageFile (pf.Url, actualFileName);
					}

				}
			}
			return t;
		}

		public async Task SaveChatItemAsync (ChatItem chatItem)
		{
			ParseObject po = ToParseObject (chatItem);
			try {
				await po.SaveAsync ();
			} catch (Exception e) {
				System.Console.WriteLine("EXCEPTION " + e);
			}
			chatItem.webId = po.ObjectId;
		}

		public async Task SaveChatItemAsync (ChatItem chatItem, ProgressBar progressBar, Activity activity)
		{
			ParseObject po = ToParseObject (chatItem);
			ParseFile pf = (ParseFile)po ["fileData"];
			activity.RunOnUiThread (() => {
				progressBar.Visibility = Android.Views.ViewStates.Visible;
			});

			await pf.SaveAsync (new Progress<ParseUploadProgressEventArgs> (e => {
				int currProgress = (int)(100 * e.Progress);
				activity.RunOnUiThread (() => {
					progressBar.Progress = currProgress;
				});
			}));
			activity.RunOnUiThread (() => {
				progressBar.Visibility = Android.Views.ViewStates.Gone;
			});
			await po.SaveAsync ();

			chatItem.webId = po.ObjectId;
		}

		public async Task GetAndSyncChatItemsAsync (string chatroomId)
		{
			var query = ParseObject.GetQuery ("Chat").WhereEqualTo ("chatroomId", chatroomId);
			var results = await query.FindAsync ();

			foreach (ParseObject chatPO in results) {
				ChatItem chat = FromParseObject (chatPO);
				if (DatabaseAccessors.ChatDatabaseAccessor.ExistsChat (chat.webId)) {
					var currItem = DatabaseAccessors.ChatDatabaseAccessor.GetItem(chat.webId);
					chat.ID = currItem.ID;
					chat.read = currItem.read;
					chat.createdAt = currItem.createdAt;
				}
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem (chat);
				
			}
		}
	}
}

