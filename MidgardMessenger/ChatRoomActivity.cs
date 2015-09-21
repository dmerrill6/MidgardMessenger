
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
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			chatroom = DatabaseAccessors.ChatRoomDatabaseAccessor.GetChatRoom (Intent.GetStringExtra ("chatroom"));
			SetContentView (Resource.Layout.ChatRoom);
			var chatsAdapter = new ChatAdapter (this);
			var chatsListView = FindViewById<ListView> (Resource.Id.chatsListView);
			chatsListView.Adapter = chatsAdapter;
			chatsListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => {

			};

			var sendButton = FindViewById<Button> (Resource.Id.send_message_button);
			sendButton.Click += async (sender, e) => {
				EditText textEdit = FindViewById<EditText>(Resource.Id.chatInputTextBox);
				string messageContent = textEdit.Text.ToString();
				textEdit.Text = "";
				ParseChatItemDatabase parseItemDB = new ParseChatItemDatabase();
				ChatItem chat = new ChatItem();
				chat.chatroomID = chatroom.webID;
				chat.senderID = DatabaseAccessors.CurrentUser().webID;
				chat.content = messageContent;

				parseItemDB.SaveChatItemAsync(chat);
				DatabaseAccessors.ChatDatabaseAccessor.SaveItem(chat);
				chatsAdapter.NotifyDataSetChanged();
			};
		}



		protected void LoadMessages()
		{
			
		}
	}


}

