
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Parse;
using Android.Graphics;
using Android.Provider;
using Uri = Android.Net.Uri;
using Android.Content.PM;
using Java.IO;

namespace MidgardMessenger
{
	[Activity (Label = "ChatRoomActivity")]			
	public class ChatRoomActivity : Activity
	{	
		public static File _file;
	    public static Bitmap bitmap;
	    public static Action<int, string> chatImageUploadProgressUpdated; 

		protected ChatRoom chatroom;
		public ChatRoom ChatRoomAccessor{
			get{
				return chatroom;
			}
		}
		protected ChatAdapter chatsAdapter;

		protected async Task SynchronizeWithParse(){
			ParseChatItemDatabase parseDB = new ParseChatItemDatabase ();
			await parseDB.GetAndSyncChatItemsAsync (chatroom.webID);
			await ParsePush.SubscribeAsync (chatroom.webID);
			RunOnUiThread (() => chatsAdapter.NotifyDataSetChanged ());
			RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));


		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (Intent.GetStringExtra ("chatroom"));
			SetContentView (Resource.Layout.ChatRoom);
			var toolbar = FindViewById<Toolbar> (Resource.Id.chatroom_toolbar);
			//Toolbar will now take on default Action Bar characteristics
			SetActionBar (toolbar);
			List<User> users = DatabaseAccessors.ChatRoomDatabaseAccessor.GetUsers(chatroom.webID).ToList();
			string chatroomName = "Untitled";
			if (users.Count > 0) {
				chatroomName = users.ElementAt (0).name;
				if (users.Count > 1 && chatroomName == DatabaseAccessors.CurrentUser ().name)
					chatroomName = users.ElementAt (1).name;
			}
			ActionBar.Title = chatroomName;
			chatsAdapter = new ChatAdapter (this);
			var chatsListView = FindViewById<ListView> (Resource.Id.chatsListView);
			chatsListView.Adapter = chatsAdapter;
			chatsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

			};

			var sendButton = FindViewById<Button> (Resource.Id.send_message_button);
			var textBoxEdit = FindViewById<EditText> (Resource.Id.chatInputTextBox);
			textBoxEdit.Click += (sender, e) => {
				RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));
			};
			sendButton.Click += async (sender, e) => {
				EditText textEdit = FindViewById<EditText>(Resource.Id.chatInputTextBox);
				string messageContent = textEdit.Text.ToString();
				RunOnUiThread( () => textEdit.Text = "");
				ParseChatItemDatabase parseItemDB = new ParseChatItemDatabase();
				ChatItem chat = new ChatItem();
				chat.chatroomID = chatroom.webID;
				chat.senderID = DatabaseAccessors.CurrentUser().webID;
				chat.content = messageContent;

				await parseItemDB.SaveChatItemAsync(chat);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chat);
				RunOnUiThread(() => chatsAdapter.NotifyDataSetChanged());
				RunOnUiThread( () => FindViewById<ListView>(Resource.Id.chatsListView).SmoothScrollToPosition(chatsAdapter.GetCount() - 1));

				var push = new ParsePush();
				push.Channels = new List<string> {chatroom.webID};
				push.Alert = "Your men might be requesting help!";
				await push.SendAsync();
			};

			Task getUpdatedInfo = new Task (async () => {
				await SynchronizeWithParse ();
			});

			ParsePush.ParsePushNotificationReceived += async (sender, args) => {
				
				await SynchronizeWithParse();
			};



			getUpdatedInfo.Start ();
		}



		private bool IsThereAnAppToTakePictures ()
		{
		    Intent intent = new Intent (MediaStore.ActionImageCapture);
		    IList<ResolveInfo> availableActivities =
		        PackageManager.QueryIntentActivities (intent, PackageInfoFlags.MatchDefaultOnly);
		    return availableActivities != null && availableActivities.Count > 0;
		}

		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.chatroom, menu);

			return base.OnCreateOptionsMenu (menu);
		}
		private void TakeAPicture ()
		{
			System.Console.WriteLine("AAAB");
		    Intent intent = new Intent (MediaStore.ActionImageCapture);
		    ChatRoomActivity._file = new File (UtilsAndConstants.ImagesDir, String.Format("MidgardPhoto_{0}.jpg", Guid.NewGuid()));
		    intent.PutExtra (MediaStore.ExtraOutput, Uri.FromFile (ChatRoomActivity._file));
		    StartActivityForResult (intent, 0);
		}

		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(ChatRoomActivity._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            ParseChatItemDatabase pcid = new ParseChatItemDatabase();
            ChatItem chatitem = new ChatItem();
            chatitem.chatroomID = chatroom.webID;
            chatitem.content = DatabaseAccessors.CurrentUser().name + " sent a photograph!";
            chatitem.fileName = ChatRoomActivity._file.Name;
			System.Console.WriteLine(ChatRoomActivity._file.Name);
            
            chatitem.pathToFile = UtilsAndConstants.ImagesDir.Path;
            chatitem.senderID = DatabaseAccessors.CurrentUser().webID;
            DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chatitem);
            chatsAdapter.NotifyDataSetChanged();
			Task saveItemAsync = new Task (async () => {
            	await pcid.SaveChatItemAsync(chatitem, chatImageUploadProgressUpdated);
        	});
        	saveItemAsync.Start();
            // Dispose of the Java side bitmap.
            GC.Collect();
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{	
			switch (item.ItemId) {
				case Resource.Id.send_file_button:
					break;
				case Resource.Id.take_a_photograph_button:
					TakeAPicture();					
					break;
			}	
		
			return base.OnOptionsItemSelected (item);

		}


		protected void LoadMessages()
		{
			
		}
	}


}

