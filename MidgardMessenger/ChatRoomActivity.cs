
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

namespace MidgardMessenger
{
	[Activity (Label = "ChatRoomActivity")]			
	public class ChatRoomActivity : Activity
	{
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
			Console.WriteLine ("Synched chatroom");


		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (Intent.GetStringExtra ("chatroom"));
			SetContentView (Resource.Layout.ChatRoom);
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
				Console.WriteLine("push");
				await SynchronizeWithParse();
			};

			getUpdatedInfo.Start ();
		}



		protected void LoadMessages()
		{
			
		}
	}


}

